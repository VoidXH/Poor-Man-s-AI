using System.Diagnostics;
using System.Text.Json.Nodes;

using VoidX.WPF;

using PoorMansAI.VoidMoA;

using Config = PoorMansAI.Configuration.Config;
using Timer = System.Threading.Timer;

namespace PoorMansAI.Engines {
    /// <summary>
    /// Image generating neural network runner using AUTOMATIC1111's Stable Diffusion WebUI.
    /// </summary>
    public class StableDiffusionWebUI : Engine {
        /// <summary>
        /// URL of the running WebUI instance.
        /// </summary>
        static string Server => "http://localhost:" + Config.webUIPort;

        /// <summary>
        /// Lock reference.
        /// </summary>
        readonly object locker = new();

        /// <summary>
        /// Running Stable Diffusion WebUI server.
        /// </summary>
        readonly Process instance;

        /// <summary>
        /// Periodically checks and forwards image generation progress.
        /// </summary>
        Timer progressReporter;

        /// <summary>
        /// Image generation is in progress.
        /// </summary>
        bool generating;

        /// <summary>
        /// Progress check is currently running - prevents calling the endpoint again if the previous call wasn't finished.
        /// </summary>
        bool progressChecked;

        /// <summary>
        /// Image generating neural network runner.
        /// </summary>
        public StableDiffusionWebUI() {
            string dir = Path.Combine(Config.webUIRoot, "system");
            Environment.SetEnvironmentVariable("PATH", string.Join(';', Path.Combine(dir, "git", "bin"), Path.Combine(dir, "python"),
                Path.Combine(dir, "python", "Scripts"), Environment.GetEnvironmentVariable("PATH")));
            Environment.SetEnvironmentVariable("PY_LIBS", Path.Combine(dir, "python", "Scripts", "Lib") + ';' +
                Path.Combine(dir, "python", "Scripts", "Lib", "site-packages"));
            Environment.SetEnvironmentVariable("PY_PIP", Path.Combine(dir, "python", "Scripts"));
            Environment.SetEnvironmentVariable("SKIP_VENV", "1");
            Environment.SetEnvironmentVariable("PIP_INSTALLER_LOCATION", Path.Combine(dir, "python", "get-pip.py"));
            Environment.SetEnvironmentVariable("TRANSFORMERS_CACHE", Path.Combine(dir, "transformers-cache"));
            Environment.SetEnvironmentVariable("PYTHON", string.Empty);
            Environment.SetEnvironmentVariable("GIT", string.Empty);
            Environment.SetEnvironmentVariable("VENV_DIR", string.Empty);
            Environment.SetEnvironmentVariable("COMMANDLINE_ARGS", $"--api --nowebui --port {Config.webUIPort} " +
                $"--ckpt-dir \"{Path.GetFullPath(Config.artists)}\" " +
                $"--embeddings-dir \"{Path.GetFullPath(Config.embeddings)}\"");
            Environment.SetEnvironmentVariable("SD_WEBUI_LOG_LEVEL", "WARNING"); // Performance + we handle it
            dir = Path.Combine(Config.webUIRoot, "webui");
            instance = Process.Start(new ProcessStartInfo {
                FileName = "cmd",
                WorkingDirectory = dir,
                Arguments = $"/C \"{Path.Combine(dir, "webui.bat")}\"",
                UseShellExecute = false
            });

            // Give 60 seconds for startup - if fails, kill it
            DateTime tryUntil = DateTime.Now + TimeSpan.FromSeconds(60);
            while (DateTime.Now < tryUntil) {
                if (HTTP.GET(Server + "/sdapi/v1/progress") != null) {
                    return;
                }
                Thread.Sleep(100);
            }
            Dispose();
            Console.Error.WriteLine("Image engine failed to start in 60 seconds.");
        }

        /// <inheritdoc/>
        public override string Generate(Command command) {
            generating = true;
            string procPrompt = PromptTransformer.Transform(command.Prompt).ToString();
            progressReporter = new Timer(ProgressCheck, command, 0, 500);
            string result = HTTP.POST(Server + "/sdapi/v1/txt2img", procPrompt, Config.imageGenTimeout);
            generating = false;
            lock (locker) {
                progressReporter?.Dispose();
                progressReporter = null;
            }
            if (result != null) {
                try {
                    return JsonNode.Parse(result)["images"][0].ToString();
                } catch (Exception e) {
                    Console.Error.WriteLine(e);
                    return null;
                }
            }
            return null;
        }

        /// <inheritdoc/>
        public override void StopGeneration() {
            HTTP.POST(Server + "/sdapi/v1/interrupt");
            lock (locker) {
                progressReporter?.Dispose();
                progressReporter = null;
            }
        }

        /// <inheritdoc/>
        public override void Dispose() {
            if (progressReporter != null) {
                StopGeneration();
            }
            while (generating) {
                Thread.Sleep(100);
            }
            instance.Kill(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Periodically check and forward image generation progress.
        /// </summary>
        void ProgressCheck(object commandIn) {
            lock (locker) {
                if (progressChecked) {
                    return;
                }
                progressChecked = true;
            }

            string result = HTTP.GET(Server + "/sdapi/v1/progress");
            if (result != null) { // null = finished
                JsonNode parsed = JsonNode.Parse(result),
                    progress = parsed["progress"];
                if (progress != null) {
                    Command command = (Command)commandIn;
                    float parsedProgress = progress.GetValue<float>();
                    string parsedResult = parsed["current_image"]?.ToString();
                    lock (locker) {
                        if (progressReporter != null && generating) {
                            UpdateProgress(command, parsedProgress, parsedResult);
                        }
                    }
                }
            }

            lock (locker) {
                progressChecked = false;
            }
        }
    }
}