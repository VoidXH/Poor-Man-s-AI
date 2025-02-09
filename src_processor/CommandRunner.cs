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
        /// Used for making sure we don't waste bandwidth for reporting the same exact result as before.
        /// </summary>
        string lastStatus;

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
                JsonNode commands = JsonNode.Parse(result);
                foreach (JsonNode entry in commands["commands"].AsArray()) {
                    int id = int.Parse(entry["id"].GetValue<string>());
                    lastStatus = null;
                    string command = entry["command"].GetValue<string>();
                    int split = command.IndexOf('|');
                    EngineType engineType = Enum.Parse<EngineType>(command[..split]);
                    string output = engine.Generate(engineType, id, command[(split + 1)..]);
                    ProgressUpdate(engineType, id, 1, output);
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
        /// Send updates to the server. Prevents DoS, but makes sure the final progress is always sent.
        /// </summary>
        void ProgressUpdate(EngineType engineType, int id, float progress, string status) {
            lock (runner) {
                if (progress <= 0.01) {
                    return; // First real update only
                }

                bool finished = progress == 1;
                if (!finished && DateTime.Now < lastMessage + TimeSpan.FromMilliseconds(Config.serverPollInterval)) {
                    return; // Prevent DoS
                }

                bool repeat = finished;
                string statusToSend = lastStatus != status ? status : null;
                lastStatus = status;
                do {
                    string commandUrl = "/commands.php?" + EnginesToUpdate();
                    string result = HTTP.POST(HTTP.Combine(Config.publicWebserver, commandUrl), [
                        new("update", id.ToString()),
                        new("result", statusToSend),
                        new("progress", Math.Floor(progress * 100).ToString(CultureInfo.InvariantCulture))
                    ], cookies);
                    repeat &= result == null;
                    if (repeat) {
                        Thread.Sleep(Config.serverPollInterval);
                    } else if (result == "STOP") { // Canceled
                        engine.StopGeneration(engineType);
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