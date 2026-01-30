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
public partial class LlamaCpp : Engine {
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
    LLModel lastModel;

    /// <summary>
    /// LLM chatbot using llama.cpp's API, loading models from the root configuration file.
    /// </summary>
    public LlamaCpp(LlamaCppSettings settings) : this(settings, settings.GetConfiguredModels()) { }

    /// <summary>
    /// LLM chatbot using llama.cpp's API.
    /// </summary>
    /// <param name="settings">How this instance is configured</param>
    /// <param name="models">Each model with its key from the website</param>
    public LlamaCpp(LlamaCppSettings settings, Dictionary<string, LLModel> models) {
        this.settings = settings;
        this.models = models;
        lastModel = models.First().Value;
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

    /// <inheritdoc/>
    public override string Generate(Command command) {
        int split = command.Prompt.IndexOf('|');
        if (!models.TryGetValue(command.Prompt[..split], out LLModel model)) {
            Logger.Error("Model not found: " + command.Prompt[..split]);
            return "Model not found.";
        }

        int timeout = settings.Timeout;
        if (lastModel.FilePath != model.FilePath) {
            lastModel = model;
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
            ["temperature"] = model.Temperature,
            ["stream"] = true
        };
        model.Jinja?.Attach(root);

        canceller = new();
        string result;
        try {
            result = HTTP.POST(Server + "/v1/chat/completions", root.ToJsonString(), x => UpdateProgress(command, .5f, x),
                Config.serverPollInterval / 3 /* final callback also limits */, Parse, canceller.Token, timeout);
        } catch (Exception e) {
            Console.Error.WriteLine(e);
            result = null;
        }

        if (model.Jinja?.ToolSelected ?? false) {
            result = model.Jinja.LaunchTool(this, UpdateProgress);
        }

        canceller.Dispose();
        canceller = null;
        Extension.RunChatPostprocessActions(messages, ref result);
        return result;
    }

    /// <summary>
    /// Parse a generation endpoint result.
    /// </summary>
    string Parse(string result) {
        if (result.Length == 0) {
            return string.Empty;
        }
        try {
            JsonNode choices = JsonNode.Parse(result[6..])["choices"];
            JsonNode delta = choices[0]["delta"];
            JsonNode toolCalls = delta["tool_calls"];
            if (toolCalls == null) {
                return delta["content"]?.ToString();
            } else {
                lastModel.Jinja.ParseToolCalls(toolCalls);
                return string.Empty;
            }
        } catch {
            return string.Empty;
        }
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
        string workingDir = settings.GPU ? Config.llamaCppGPURoot : Config.llamaCppCPURoot;
        ProcessStartInfo info = new() {
            WorkingDirectory = workingDir,
            Arguments = $"-m \"{lastModel.FilePath}\" --port {settings.Port} -c {settings.Context} --keep {settings.Keep}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        if (settings.GPU) {
            info.Arguments += " -ngl 999";
        }
        if (Config.chatLocalhost) {
            info.Arguments += " --host 0.0.0.0";
        }
        if (OperatingSystem.IsWindows()) {
            info.FileName = Path.Combine(workingDir, "llama-server.exe");
        } else {
            info.FileName = Path.Combine(workingDir, "llama-server");
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
        lastModel = null;
        return instance;
    }
}
