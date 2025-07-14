using System.Diagnostics;
using System.Text.Json.Nodes;

using VoidX.WPF;

using PoorMansAI.NewTech.VoidMoA;

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
        readonly Watchdog runner;

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
        /// Generation was done at least once. If it's a cold run, add extra timeout.
        /// </summary>
        bool ranOnce;

        /// <summary>
        /// Image generating neural network runner.
        /// </summary>
        public StableDiffusionWebUI() => runner = new(Launch);

        /// <summary>
        /// Start a new Stable Diffusion WebUI instance.
        /// </summary>
        public static Process Launch() {
            string root = Path.GetFullPath(Config.webUIRoot);
            string[] foldersInRoot = Directory.GetDirectories(root);
            if (foldersInRoot.Length == 1) {
                root = foldersInRoot[0];
            }

            string dir = Path.Combine(root, "webui");
            string args = $"--api --nowebui --port {Config.webUIPort} " +
                $"--ckpt-dir \"{Path.GetFullPath(Config.artists)}\" " +
                $"--embeddings-dir \"{Path.GetFullPath(Config.embeddings)}\"";
            if (OperatingSystem.IsWindows()) {
                Environment.SetEnvironmentVariable("COMMANDLINE_ARGS", args);
            } else {
                string userConfigPath = Path.Combine(dir, "webui-user.sh"),
                    userConfig = $"export COMMANDLINE_ARGS=\"{macArgs} {args.Replace('"', '\'')}\"";
                if (File.ReadAllText(userConfigPath) != userConfig) {
                    File.WriteAllText(userConfigPath, userConfig);
                }
            }

            ProcessStartInfo info = new() {
                WorkingDirectory = dir,
                UseShellExecute = false
            };
            if (OperatingSystem.IsWindows()) {
                SetWindowsEnvironment(root);
                info.FileName = Path.Combine(dir, "webui.bat");
            } else {
                Process chmod = Process.Start(new ProcessStartInfo() {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"chmod +x '{Path.Combine(dir, "webui.sh")}'\"",
                    UseShellExecute = false
                });
                chmod.WaitForExit();
                info.FileName = Path.Combine(dir, "webui.sh");
            }
            Process instance = Process.Start(info);

            // Wait for startup
            DateTime tryUntil = DateTime.Now + TimeSpan.FromSeconds(60);
            while (true) {
                if (DateTime.Now >= tryUntil) {
                    Logger.Warning("Image engine failed to start in 60 seconds.");
                    tryUntil = DateTime.MaxValue;
                }
                if (HTTP.GET(Server + "/sdapi/v1/progress") != null) {
                    return instance;
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Setup the batch environment for the <paramref name="root"/> folder of Stable Diffusion WebUI.
        /// </summary>
        static void SetWindowsEnvironment(string root) {
            string dir = Path.Combine(root, "system");
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
            Environment.SetEnvironmentVariable("SD_WEBUI_LOG_LEVEL", "WARNING"); // Performance + we handle it
        }

        /// <inheritdoc/>
        public override string Generate(Command command) {
            generating = true;
            TransformedPrompt transformedPrompt = PromptTransformer.Transform(command.Prompt);
            string procPrompt = transformedPrompt.ToString();
            progressReporter = new Timer(ProgressCheck, command, 0, 500);

            int timeout = Config.imageGenTimeout;
            if (!ranOnce) {
                timeout += Config.imageGenLoading;
                ranOnce = true;
            }
            if (transformedPrompt.ReferenceImages?.Length > 0) {
                timeout += Config.imageGenParsing;
            }
            string result = HTTP.POST($"{Server}/sdapi/v1/{transformedPrompt.Endpoint}", procPrompt, timeout);
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
            runner.Dispose();
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

        /// <summary>
        /// Recommended command line arguments when Stable Diffusion WebUI is ran on a Mac.
        /// </summary>
        const string macArgs = "--skip-torch-cuda-test --upcast-sampling --use-cpu interrogate";
    }
}