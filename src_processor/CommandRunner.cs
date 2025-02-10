using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;

using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Engines;

using Timer = System.Threading.Timer;

namespace PoorMansAI {
    /// <summary>
    /// Processes the commands of the server's command queue.
    /// </summary>
    public class CommandRunner : IDisposable {
        /// <summary>
        /// Provides access to the running engines.
        /// </summary>
        readonly EngineCache engine;

        /// <summary>
        /// Cookies received when logged in successfully.
        /// </summary>
        readonly CookieCollection cookies;

        /// <summary>
        /// Periodically checks the command queue.
        /// </summary>
        readonly Timer runner;

        /// <summary>
        /// Command processing in progress, don't allow the <see cref="runner"/> to perform another event.
        /// </summary>
        bool executing;

        /// <summary>
        /// Don't allow progress updates to the server too often.
        /// </summary>
        DateTime lastMessage;

        /// <summary>
        /// Processes the commands of the server's command queue.
        /// </summary>
        public CommandRunner() {
            // Login
            cookies = HTTP.GetCookies(HTTP.Combine(Config.publicWebserver, "/commands.php"), [
                new KeyValuePair<string, string>("username", Config.adminUsername),
                new KeyValuePair<string, string>("password", Config.adminPassword)
            ]);

            // Start command processing
            engine = new(cookies);
            engine.OnProgress += ProgressUpdate;
            runner = new(ProcessCommands, null, 0, Config.serverPollInterval);
        }

        /// <summary>
        /// Periodically checks the command queue.
        /// </summary>
        void ProcessCommands(object _) {
            lock (runner) {
                if (executing) {
                    return;
                }
                executing = true;
            }

            string commandUrl = "/commands.php?list&" + EnginesToUpdate();
            string result = HTTP.GET(HTTP.Combine(Config.publicWebserver, commandUrl), cookies);
            if (result != null) {
                try {
                    JsonNode parsed = JsonNode.Parse(result);
                    IEnumerable<Command> commands = parsed["commands"].AsArray().Select(x => new Command(x));
                    IEnumerable<IGrouping<EngineType, Command>> groupsPre = commands.GroupBy(command => command.EngineType);
                    IEnumerable<(EngineType engine, Command[] commands)> groups = groupsPre.Select(g => (g.Key, g.ToArray()));
                    Command[] cpu = groups.FirstOrDefault(x => RunsOnCPU(x.engine)).commands;
                    Command[] gpu = groups.FirstOrDefault(x => RunsOnGPU(x.engine)).commands;
                    Command[] control = groups.Where(x => !RunsOnCPU(x.engine) && !RunsOnGPU(x.engine)).SelectMany(x => x.commands).ToArray();

                    RunGroup(control);
                    Task onCPU = null;
                    if (cpu != null) {
                        onCPU = Task.Run(() => RunGroup(cpu));
                    }
                    Task onGPU = null;
                    if (gpu != null) {
                        onGPU = Task.Run(() => RunGroup(gpu));
                    }
                    try {
                        onCPU?.Wait();
                    } catch (Exception e) { // If CPU fails, GPU could still run
                        Console.Error.WriteLine(e);
                    }
                    try {
                        onGPU?.Wait();
                    } catch (Exception e) {
                        Console.Error.WriteLine(e);
                    }
                } catch (Exception e) {
                    Console.Error.WriteLine(e);
                }
            }

            lock (runner) {
                executing = false;
            }
        }

        /// <summary>
        /// Dispose all instances used for command handling.
        /// </summary>
        public void Dispose() {
            engine.Dispose();
            runner.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Process a specific set of <paramref name="commands"/> on their corresponding engine.
        /// </summary>
        void RunGroup(Command[] commands) {
            for (int i = 0; i < commands.Length; i++) {
                string output = engine.Generate(commands[i]);
                ProgressUpdate(commands[i], 1, output);
            }
        }

        /// <summary>
        /// Returns if an <see cref="EngineType"/> can only run on the CPU.
        /// </summary>
        bool RunsOnCPU(EngineType engine) {
            return engine == EngineType.Chat;
        }

        /// <summary>
        /// Returns if an <see cref="EngineType"/> can run on the GPU.
        /// </summary>
        bool RunsOnGPU(EngineType engine) {
            return engine == EngineType.Image;
        }

        /// <summary>
        /// Send updates to the server. Prevents DoS, but makes sure the final progress is always sent.
        /// </summary>
        void ProgressUpdate(Command command, float progress, string status) {
            lock (runner) {
                if (progress <= 0.01) {
                    return; // First real update only
                }

                bool finished = progress == 1;
                if (!finished && DateTime.Now < lastMessage + TimeSpan.FromMilliseconds(Config.serverPollInterval)) {
                    return; // Prevent DoS
                }

                bool repeat = finished;
                do {
                    string commandUrl = "/commands.php?" + EnginesToUpdate();
                    string result = HTTP.POST(HTTP.Combine(Config.publicWebserver, commandUrl), [
                        new("update", command.ID.ToString()),
                        new("result", command.Update(status)),
                        new("progress", Math.Floor(progress * 100).ToString(CultureInfo.InvariantCulture))
                    ], cookies);
                    repeat &= result == null;
                    if (repeat) {
                        Thread.Sleep(Config.serverPollInterval);
                    } else if (result == "STOP") { // Canceled
                        engine.StopGeneration(command.EngineType);
                    }
                } while (repeat);
                lastMessage = DateTime.Now;
            }
        }

        /// <summary>
        /// The commands.php endpoint updates the last available time of running engines. These are the GET parameters to update all running engines.
        /// </summary>
        string EnginesToUpdate() {
            string result = string.Empty;
            if (engine.CanGenerateText) {
                result += "llm";
            }
            if (engine.CanGenerateImages) {
                if (result.Length != 0) result += '&';
                result += "moa";
            }
            return result;
        }
    }
}