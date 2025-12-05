using System.Diagnostics;
using System.Text.Json.Nodes;

using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Extensions;
using PoorMansAI.NewTech.ContextDocTree;
using PoorMansAI.Engines.Orchestration;
using PoorMansAI.Engines.Models;

namespace PoorMansAI.Engines;

/// <summary>
/// LLM chatbot using llama.cpp's API.
/// </summary>
public class LlamaCpp : Engine {
    /// <summary>
    /// The model currently runs on the GPU.
    /// </summary>
    public bool GPU => settings.GPU;

    /// <summary>
    /// URL of the running llama.cpp instance.
    /// </summary>
    string Server => "http://localhost:" + settings.Port;

    /// <summary>
    /// How this instance is configured.
    /// </summary>
    readonly LlamaCppSettings settings;

    /// <summary>
    /// Path of each selectable model.
    /// </summary>
    readonly Dictionary<string, LLModel> models;

    /// <summary>
    /// Handles context documents.
    /// </summary>
    readonly ContextDocFinder contextDocs = new();

    /// <summary>
    /// Running llama.cpp server.
    /// </summary>
    Watchdog runner;

    /// <summary>
    /// Can cancel generation.
    /// </summary>
    CancellationTokenSource canceller;

    /// <summary>
    /// Used model in the running <see cref="instance"/>.
    /// </summary>
    string lastModelPath;

    /// <summary>
    /// LLM chatbot using llama.cpp's API.
    /// </summary>
    /// <param name="settings">How this instance is configured</param>
    /// <param name="models">Each model with its key from the website</param>
    public LlamaCpp(LlamaCppSettings settings, Dictionary<string, LLModel> models) {
        this.settings = settings;
        this.models = models;
        lastModelPath = models.First().Value.FilePath;
        runner = new(Launch);
    }

    /// <summary>
    /// Make the system ready to launch llama-server.exe.
    /// </summary>
    public static void Ready() {
        Process[] processes = Process.GetProcessesByName("llama-server");
        foreach (var process in processes) {
            process.Kill();
            process.WaitForExit();
            Logger.Warning("A previously stuck llama-server instance was killed.");
        }
    }

    /// <summary>
    /// Parse a generation endpoint result.
    /// </summary>
    static string Parse(string result) {
        if (result.Length == 0) {
            return string.Empty;
        }
        try {
            return JsonNode.Parse(result[6..])["choices"][0]["delta"]["content"]?.ToString();
        } catch {
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public override string Generate(Command command) {
        int split = command.Prompt.IndexOf('|');
        LLModel model = models[command.Prompt[..split]];
        int timeout = settings.Timeout;
        if (lastModelPath != model.FilePath) {
            lastModelPath = model.FilePath;
            timeout += settings.Loading;
            runner.Dispose();
            runner = new(Launch);
        }

        JsonArray messages = [];
        messages.Add(new JsonObject {
            ["role"] = "system",
            ["content"] = Config.augmentWithSystemPrompt ? model.SystemMessage : contextDocs.TransformPrompt(model.SystemMessage)
        });

        string[] chat = command.Prompt[(split + 1)..].Split('|');
        if (model.PostMessage != null) {
            chat[^1] += model.PostMessage;
        }

        for (int i = 0, last = chat.Length - 1; i <= last; i++) {
            bool user = i % 2 == 0;
            string message = chat[i].Replace("&vert;", "|").Replace("\"", "\\\"");
            if (user && (!Config.augmentLatestOnly || i == last)) {
                message = contextDocs.TransformPrompt(message, Config.augmentWithSystemPrompt ? model.SystemMessage : null);
            }
            messages.Add(new JsonObject {
                ["role"] = user ? "user" : "assistant",
                ["content"] = message
            });
        }

        JsonObject root = new() {
            ["model"] = "gpt-3.5-turbo",
            ["max_tokens"] = settings.Predict,
            ["messages"] = messages,
            ["n_discard"] = settings.Discard,
            ["stream"] = true
        };

        canceller = new();
        string result;
        try {
            result = HTTP.POST(Server + "/v1/chat/completions", root.ToJsonString(), x => UpdateProgress(command, .5f, x),
                Config.serverPollInterval / 3 /* final callback also limits */, Parse, canceller.Token, timeout);
        } catch (Exception e) {
            Console.Error.WriteLine(e);
            result = null;
        }
        canceller.Dispose();
        canceller = null;
        Extension.RunChatPostprocessActions(messages, ref result);
        return result;
    }

    /// <inheritdoc/>
    public override void StopGeneration() => canceller?.Cancel();

    /// <inheritdoc/>
    public override void Dispose() {
        runner.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Relaunch with the currently selected <see cref="lastModelPath"/>.
    /// </summary>
    Process Launch() {
        string workingDir = settings.GPU ? Config.llamaCppGPURoot : Config.llamaCppCPURoot,
            ngl = settings.GPU ? " -ngl 999" : string.Empty;
        ProcessStartInfo info = new() {
            WorkingDirectory = workingDir,
            Arguments = $"-m \"{lastModelPath}\" --port {settings.Port} -c {settings.Context} --keep {settings.Keep}{ngl}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        if (OperatingSystem.IsWindows()) {
            info.FileName = Path.Combine(workingDir, "llama-server.exe");
        } else {
            info.FileName = Path.Combine(workingDir, "build/bin/llama-server");
        }
        Logger.Debug("Llama.cpp launched with: " + info.Arguments);
        Process instance = Process.Start(info);
        instance.ErrorDataReceived += SanitizeLog;
        instance.OutputDataReceived += SanitizeLog;
        instance.BeginErrorReadLine();
        instance.BeginOutputReadLine();

        // Give 30 seconds for startup - if fails, kill it
        DateTime tryUntil = DateTime.Now + TimeSpan.FromSeconds(30);
        while (DateTime.Now < tryUntil) {
            if (HTTP.GET(Server + "/health")?.Contains("ok") ?? false) {
                return instance;
            }
            Thread.Sleep(100);
        }
        lastModelPath = null;
        return instance;
    }

    /// <summary>
    /// Selectively print llama.cpp logs based on the current log level.
    /// </summary>
    void SanitizeLog(object _, DataReceivedEventArgs e) {
        if (e.Data == null) {
            return;
        }

        string line = e.Data;
        if (Logger.MinLogLevel > LogLevel.Debug) {
            for (int i = 0; i < skippedLineStarts.Length; i++) {
                if (line.StartsWith(skippedLineStarts[i])) {
                    return;
                }
            }
            for (int i = 0; i < skippedLineEnds.Length; i++) {
                if (line.EndsWith(skippedLineEnds[i])) {
                    return;
                }
            }

            int index = line.IndexOf('{');
            if (index != -1 && index + 1 != line.Length && (line[index + 1] == '{' || line[index + 1] == '%' || line[index + 1] == '#')) {
                return;
            }
            index = line.LastIndexOf('}');
            if (index > 0 && (line[index - 1] == '%' || line[index - 1] == '}')) {
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(line)) {
            Logger.Log("llama.cpp", line, ConsoleColor.DarkCyan, false);
        }
    }

    /// <summary>
    /// Lines starting with these are only logged when <see cref="Logger.MinLogLevel"/> is lower or equal than <see cref="LogLevel.Debug"/>.
    /// </summary>
    static readonly string[] skippedLineStarts = [
        ".....", ", example_format:", "<start_of_turn>", "<|im_start|>",
        "build:",
        "common_init_from_params:",
        "ggml_metal_init:",
        "llama_context:", "llama_kv_cache:", "llama_kv_cache_iswa:", "llama_model_loader:", "load_backend:", "load_tensors:",
        "print_info:",
        "slot ", "srv  ", "system info:", "system_info:"
    ];

    /// <summary>
    /// Lines ending with these are only logged when <see cref="Logger.MinLogLevel"/> is lower or equal than <see cref="LogLevel.Debug"/>.
    /// </summary>
    static readonly string[] skippedLineEnds = [
        "<end_of_turn>", "<|im_end|>"
    ];
}
