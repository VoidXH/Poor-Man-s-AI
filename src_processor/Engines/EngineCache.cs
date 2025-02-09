using System.Net;

using VoidX.WPF;

using PoorMansAI.Configuration;

namespace PoorMansAI.Engines {
    /// <summary>
    /// Keeps <see cref="Engine"/>s by certain rules active and reusable. Sends commands through cached instances, switches running software if needed.
    /// </summary>
    public class EngineCache : IDisposable {
        /// <summary>
        /// Rule to keep engines alive by.
        /// </summary>
        public EngineCacheMode Mode {
            get => mode;
            set {
                if (mode == value) {
                    return;
                }

                // Shutdowns
                bool slm = (value & EngineCacheMode.SLM) != 0;
                if (chatEngine?.LLM == true || !slm) {
                    chatEngine?.Dispose();
                    chatEngine = null;
                }

                bool llm = (value & EngineCacheMode.LLM) != 0;
                if (chatEngine?.LLM == false || !llm) {
                    chatEngine?.Dispose();
                    chatEngine = null;
                }

                bool moa = (value & EngineCacheMode.Image) != 0;
                if (!moa) {
                    imageEngine?.Dispose();
                    imageEngine = null;
                }

                // Launches
                if (moa) {
                    if (imageEngine == null) {
                        imageEngine = new StableDiffusionWebUI();
                        imageEngine.OnProgress += CallOnProgress;
                    }
                }

                if (slm && chatEngine == null) {
                    chatEngine = new LlamaCpp(false);
                    chatEngine.OnProgress += CallOnProgress;
                }

                if (llm && chatEngine == null) {
                    chatEngine = new LlamaCpp(true);
                    chatEngine.OnProgress += CallOnProgress;
                }

                mode = value;
            }
        }
        EngineCacheMode mode;

        /// <summary>
        /// An LLM is running.
        /// </summary>
        public bool CanGenerateText => chatEngine != null;

        /// <summary>
        /// An image generator is running.
        /// </summary>
        public bool CanGenerateImages => imageEngine != null;

        /// <summary>
        /// Reports partial progression and midway states.
        /// </summary>
        public Engine.Progress OnProgress;

        /// <summary>
        /// Authentication cookies to the server.
        /// </summary>
        readonly CookieCollection cookies;

        /// <summary>
        /// Active chatbot instance, could be CPU (SLM mode) or GPU (LLM mode).
        /// </summary>
        LlamaCpp chatEngine;

        /// <summary>
        /// Active image generator, possibly takes up the entire GPU.
        /// </summary>
        StableDiffusionWebUI imageEngine;

        /// <summary>
        /// On launch, the system default mode is used and sent to the server for activation.
        /// </summary>
        public EngineCache(CookieCollection cookies) {
            this.cookies = cookies;
            string mode = HTTP.GET(HTTP.Combine(Config.publicWebserver, "/models.php?active"), cookies);
            try {
                Mode = (EngineCacheMode)int.Parse(mode);
            } catch {
                Console.Error.WriteLine("Invalid login credentials.");
                throw;
            }
        }

        /// <summary>
        /// Run a <paramref name="command"/> of a given <paramref name="id"/> through the corresponding <paramref name="engine"/>.
        /// </summary>
        /// <returns>The output of the generation.</returns>
        public string Generate(Command command) {
            if (command.EngineType == EngineType.Mode) {
                Mode = (EngineCacheMode)int.Parse(command.Prompt);
                HTTP.POST(HTTP.Combine(Config.publicWebserver, "/models.php"), [
                    new KeyValuePair<string, string>("active", command.Prompt)
                ], cookies);
                return null;
            }

            return command.EngineType switch {
                EngineType.Chat => chatEngine?.Generate(command) ?? "Chat engine is not loaded.",
                EngineType.Image => imageEngine?.Generate(command) ?? File.ReadAllText("Data/OfflineImage.txt"),
                _ => null
            };
        }

        /// <summary>
        /// Cancel the current operation.
        /// </summary>
        public void StopGeneration(EngineType type) {
            Engine engine = type switch {
                EngineType.Chat => chatEngine,
                EngineType.Image => imageEngine,
                _ => null
            };
            engine?.StopGeneration();
        }

        /// <inheritdoc/>
        public void Dispose() {
            chatEngine?.Dispose();
            imageEngine?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allow for lazy subscriptions.
        /// </summary>
        void CallOnProgress(Command command, float progress, string status) => OnProgress?.Invoke(command, progress, status);
    }
}