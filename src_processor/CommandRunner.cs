using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Engines;
using PoorMansAI.Engines.Orchestration;

namespace PoorMansAI;

/// <summary>
/// Processes the commands of the server's command queue.
/// </summary>
public class CommandRunner : IDisposable {
    /// <summary>
    /// Cookies received when logged in successfully.
    /// </summary>
    internal CookieCollection cookies;

    /// <summary>
    /// Provides access to the running engines.
    /// </summary>
    readonly EngineCache engine;

    /// <summary>
    /// Periodically checks the command queue.
    /// </summary>
    readonly Thread runner;

    /// <summary>
    /// Stops the command <see cref="runner"/> on <see cref="Dispose"/>.
    /// </summary>
    readonly CancellationTokenSource canceller;

    /// <summary>
    /// Don't allow progress updates to the server too often.
    /// </summary>
    DateTime lastMessage;

    /// <summary>
    /// Processes the commands of the server's command queue.
    /// </summary>
    public CommandRunner() {
        Login();
        engine = new(cookies);
        engine.OnProgress += ProgressUpdate;
        canceller = new();
        runner = new(new ThreadStart(ProcessCommands));
        runner.Start();
    }

    /// <summary>
    /// Returns if an <see cref="EngineType"/> can only run on the CPU.
    /// </summary>
    static bool RunsOnCPU(EngineType engine) => engine == EngineType.Chat;

    /// <summary>
    /// Returns if an <see cref="EngineType"/> can run on the GPU.
    /// </summary>
    static bool RunsOnGPU(EngineType engine) => engine == EngineType.Image;

    /// <summary>
    /// Dispose all instances used for command handling.
    /// </summary>
    public void Dispose() {
        engine.Dispose();
        canceller.Cancel();
        runner.Join();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Log in to the server and store the session cookies.
    /// </summary>
    void Login() => cookies = HTTP.GetCookies(HTTP.Combine(Config.publicWebserver, "/login.php"), [
        new KeyValuePair<string, string>("name", Config.adminUsername),
        new KeyValuePair<string, string>("password", Config.adminPassword)
    ]);

    /// <summary>
    /// Periodically checks the command queue.
    /// </summary>
    void ProcessCommands() {
        while (!canceller.IsCancellationRequested) {
            DateTime processingStarted = DateTime.Now;
            string commandUrl = "/cmd/list.php?" + EnginesToUpdate(),
                result = HTTP.GET(HTTP.Combine(Config.publicWebserver, commandUrl), cookies);
            if (result == null) {
                Logger.Debug("Couldn't query the list of commands from the Website, waiting 10 seconds.");
                Thread.Sleep(10000);
            } else if (result.Length == 0) {
                Logger.Debug("Cookies expired, logging in again.");
                try {
                    Login();
                } catch {
                    Logger.Info("Login failed, trying again.");
                }
            } else {
                try {
                    JsonNode parsed = JsonNode.Parse(result);
                    IEnumerable<Command> commands = parsed["commands"].AsArray().Select(x => new Command(x));

                    if (commands.Any()) {
                        if (Config.unified) {
                            RunGroup(commands);
                        } else {
                            RunParallel(commands);
                        }
                        Logger.Debug("All commands processed.");
                    } else {
                        Logger.Debug("No new commands.");
                    }
                } catch (Exception e) {
                    Logger.Error("Error while processing commands: " + e);
                    Logger.Debug("Server response that resulted in an error:\n{0}", result);
                }
            }

            int waitFosMS = Config.serverPollInterval - (int)(DateTime.Now - processingStarted).TotalMilliseconds;
            if (waitFosMS > 0) {
                Thread.Sleep(waitFosMS);
            }
        }
    }

    /// <summary>
    /// Process a specific set of <paramref name="commands"/> on their corresponding engine.
    /// </summary>
    void RunGroup(IEnumerable<Command> commands) {
        foreach (Command command in commands) {
            string output = engine.Generate(command) ?? "Error.";
            ProgressUpdate(command, 1, output);
        }
    }

    /// <summary>
    /// Run <see cref="Command"/>s at the same time on the CPU and on the GPU.
    /// </summary>
    void RunParallel(IEnumerable<Command> commands) {
        IEnumerable<IGrouping<EngineType, Command>> groupsPre = commands.GroupBy(command => command.EngineType);
        IEnumerable<(EngineType engine, Command[] commands)> groups = groupsPre.Select(g => (g.Key, g.ToArray()));
        Command[] cpu = groups.FirstOrDefault(x => RunsOnCPU(x.engine)).commands;
        Command[] gpu = groups.FirstOrDefault(x => RunsOnGPU(x.engine)).commands;
        Command[] control = [.. groups.Where(x => !RunsOnCPU(x.engine) && !RunsOnGPU(x.engine)).SelectMany(x => x.commands)];

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
                string commandUrl = "/cmd/update.php?" + EnginesToUpdate();
                string result = HTTP.POST(HTTP.Combine(Config.publicWebserver, commandUrl), [
                    new("id", command.ID.ToString()),
                    new("result", command.Update(status, repeat)),
                    new("progress", Math.Floor(progress * 100).ToString(CultureInfo.InvariantCulture))
                ], cookies);

                if (result != null && result.StartsWith("RETRY", StringComparison.InvariantCulture)) {
                    Logger.Warning("Server asked for the result to be sent again. Reason: " + result[5..].TrimStart());
                    repeat = true;
                }

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
    /// Some endpoints update the last available time of running engines. These are the GET parameters to update all running engines.
    /// </summary>
    string EnginesToUpdate() {
        StringBuilder result = new();
        if (engine.CanGenerateText && Config.chatWeight >= 0) {
            result.Append("llm=").Append(Config.chatWeight);
        }
        if (engine.CanGenerateImages && Config.imageGenWeight >= 0) {
            if (result.Length != 0) {
                result.Append('&');
            }
            result.Append("moa=").Append(Config.imageGenWeight);
        }
        return result.ToString();
    }
}
