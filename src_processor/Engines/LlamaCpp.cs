﻿using System.Diagnostics;
using System.Text.Json.Nodes;

using VoidX.WPF;

using PoorMansAI.Configuration;
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
        readonly Dictionary<string, (string path, string systemMessage)> models = [];

        /// <summary>
        /// Handles context documents.
        /// </summary>
        readonly ContextDocFinder contextDocs = new();

        /// <summary>
        /// Running llama.cpp server.
        /// </summary>
        Process instance;

        /// <summary>
        /// Can cancel generation.
        /// </summary>
        CancellationTokenSource canceller;

        /// <summary>
        /// Used model in the running <see cref="instance"/>.
        /// </summary>
        string model;

        /// <summary>
        /// LLM chatbot using llama.cpp's API.
        /// </summary>
        /// <param name="llm">Use the larger models on the GPU instead of the small ones on the CPU</param>
        public LlamaCpp(bool llm) {
            Dictionary<string, string> config = Config.Values;
            string root = Path.GetFullPath(Config.models),
                postfix = llm ? "LLM" : "SLM";
            foreach (string prefix in Config.ForEachModel()) {
                string modelFile = Path.GetFileName(config[prefix + postfix]);
                models[config[prefix]] = (Path.Combine(root, modelFile), config[prefix + "SystemMessage"]);
            }

            LLM = llm;
            model = models[config["Model1"]].path;
            Launch();
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
            (string modelPath, string systemMessage) = models[command.Prompt[..split]];
            if (model != modelPath) {
                model = modelPath;
                instance.Kill(true);
                Launch();
            }

            JsonArray messages = [];
            messages.Add(new JsonObject {
                ["role"] = "system",
                ["content"] = Config.augmentWithSystemPrompt ? systemMessage : contextDocs.TransformPrompt(systemMessage)
            });

            string[] chat = command.Prompt[(split + 1)..].Split('|');
            for (int i = 0, last = chat.Length - 1; i <= last; i++) {
                bool user = i % 2 == 0;
                string message = chat[i].Replace("&vert;", "|").Replace("\"", "\\\"");
                if (user && (!Config.augmentLatestOnly || i == last)) {
                    message = contextDocs.TransformPrompt(message, Config.augmentWithSystemPrompt ? systemMessage : null);
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
                    Config.serverPollInterval / 3 /* final callback also limits */, Parse, canceller.Token, Config.chatTimeout);
            } catch (Exception e) {
                Console.Error.WriteLine(e);
                result = null;
            }
            canceller.Dispose();
            canceller = null;
            return result;
        }

        /// <inheritdoc/>
        public override void StopGeneration() => canceller?.Cancel();

        /// <inheritdoc/>
        public override void Dispose() {
            instance.Kill(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Relaunch with the currently selected <see cref="model"/>.
        /// </summary>
        void Launch() {
            string workingDir = LLM ? Config.llamaCppGPURoot : Config.llamaCppCPURoot,
                ngl = LLM ? " -ngl 999" : string.Empty,
                args = $"-m \"{model}\" --port {Config.Values["LlamaCppPort"]}{ngl}";
            ProcessStartInfo info = new() {
                WorkingDirectory = workingDir,
                UseShellExecute = false
            };
            if (OperatingSystem.IsWindows()) {
                info.FileName = "cmd";
                info.Arguments = $"/C llama-server.exe " + args;
            } else {
                info.FileName = "bash";
                info.Arguments = $"-c './llama-server' " + args;
            }
            instance = Process.Start(info);

            // Give 30 seconds for startup - if fails, kill it
            DateTime tryUntil = DateTime.Now + TimeSpan.FromSeconds(30);
            while (DateTime.Now < tryUntil) {
                if (HTTP.GET(Server + "/health")?.Contains("ok") ?? false) {
                    return;
                }
                Thread.Sleep(100);
            }
            model = null;
        }
    }
}