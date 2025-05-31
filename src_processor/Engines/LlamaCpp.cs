using System.Diagnostics;
using System.Text.Json.Nodes;

using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Extensions;
using PoorMansAI.NewTech.ContextDocTree;

namespace PoorMansAI.Engines {
    /// <summary>
    /// LLM chatbot using llama.cpp's API.
    /// </summary>
    public class LlamaCpp : Engine {
        /// <summary>
        /// Use the larger models on the GPU instead of the small ones on the CPU.
        /// </summary>
        public bool LLM { get; private set; }

        /// <summary>
        /// URL of the running llama.cpp instance.
        /// </summary>
        static string Server => "http://localhost:" + Config.Values["LlamaCppPort"];

        /// <summary>
        /// Path of each selectable model.
        /// </summary>
        readonly Dictionary<string, LLModel> models = [];

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
        /// <param name="llm">Use the larger models on the GPU instead of the small ones on the CPU</param>
        public LlamaCpp(bool llm) {
            Dictionary<string, string> config = Config.Values;
            foreach (string prefix in Config.ForEachModel()) {
                LLModel model = new(prefix, llm);
                models[model.Name] = model;
            }

            LLM = llm;
            lastModelPath = models[config["Model1"]].FilePath;
            runner = new(Launch);
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
            int timeout = Config.chatTimeout;
            if (lastModelPath != model.FilePath) {
                lastModelPath = model.FilePath;
                timeout += Config.chatLoading;
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
                ["messages"] = messages,
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
            string workingDir = LLM ? Config.llamaCppGPURoot : Config.llamaCppCPURoot,
                ngl = LLM ? " -ngl 999" : string.Empty;
            ProcessStartInfo info = new() {
                WorkingDirectory = workingDir,
                Arguments = $"-m \"{lastModelPath}\" --port {Config.Values["LlamaCppPort"]}{ngl}",
                UseShellExecute = false
            };
            if (OperatingSystem.IsWindows()) {
                info.FileName = Path.Combine(workingDir, "llama-server.exe");
            } else {
                info.FileName = Path.Combine(workingDir, "build/bin/llama-server");
            }
            Logger.Debug("Llama.cpp launched with: " + info.Arguments);
            Process instance = Process.Start(info);

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
    }
}