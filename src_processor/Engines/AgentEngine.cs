using System.Diagnostics;
using System.Text;

using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Engines.BaseClasses;
using PoorMansAI.Engines.Models;
using PoorMansAI.Engines.Utilities;

namespace PoorMansAI.Engines;

/// <summary>
/// LLM chatbot running an external CLI agent per prompt.
/// </summary>
public partial class AgentEngine : Engine {
    /// <summary>
    /// How this instance is configured.
    /// </summary>
    readonly AgentSettings settings;

    /// <summary>
    /// Safely handles <see cref="canceller"/> from multiple threads.
    /// </summary>
    readonly object cancellerLock = new();

    /// <summary>
    /// Stops the generation when cancellation is triggered or a timeout is reached.
    /// </summary>
    CancellationTokenSource canceller;

    /// <summary>
    /// The currently working agent's handle.
    /// </summary>
    Process instance;

    /// <summary>
    /// LLM chatbot running an external CLI agent per prompt.
    /// </summary>
    /// <param name="settings">How this instance is configured</param>
    public AgentEngine(AgentSettings settings) {
        this.settings = settings;
        LaunchQueueThread();
    }

    /// <summary>
    /// Cut the beginning of the <paramref name="output"/> of an agent to make it fit into the <see cref="uploadLimit"/>.
    /// </summary>
    static string CutOutput(string output) {
        string result = output.Trim();
        if (result.Length > uploadLimit) {
            string window = result[^uploadLimit..];
            int split = window.IndexOf('\n');
            if (split != -1) {
                result = result[(split + 1)..];
            }
        }
        return result;
    }

    /// <inheritdoc/>
    public override string Generate(Command command) {
        int splitter = command.Prompt.IndexOf('|');
        string workingDir;
        string prompt;
        if (splitter >= 0) {
            workingDir = command.Prompt[..splitter].Trim();
            prompt = command.Prompt[(splitter + 1)..].Trim();
        } else {
            workingDir = "";
            prompt = command.Prompt;
        }

        if (string.IsNullOrEmpty(workingDir) || !Directory.Exists(workingDir)) {
            workingDir = Environment.CurrentDirectory;
        }

        StringBuilder output = new();
        if (prompt.StartsWith('[')) {
            int commandClose = prompt.IndexOf(']', StringComparison.Ordinal);
            if (commandClose > 0) {
                string extraCommand = prompt[1..commandClose];
                string result = ExtraCommandHandler(workingDir, extraCommand, command.ID);
                if (!string.IsNullOrEmpty(result)) {
                    output.AppendLine(result);
                    prompt = prompt[(commandClose + 1)..];
                }
            }
        }
        if (string.IsNullOrEmpty(prompt)) {
            return output.ToString();
        }

        RunAgentEngine(workingDir, prompt, output, () => UpdateProgress(command, .5f, CutOutput(output.ToString())));
        return CutOutput(output.ToString());
    }

    /// <inheritdoc/>
    public override void StopGeneration() {
        lock (cancellerLock) {
            canceller?.Cancel();
        }
    }

    /// <inheritdoc/>
    public override void Dispose() {
        StopGeneration();
        StopQueueThread();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Perform a <paramref name="prompt"/> in a <paramref name="workingDir"/> on the agent engine, and put its result in the <paramref name="output"/>.
    /// </summary>
    void RunAgentEngine(string workingDir, string prompt, StringBuilder output, Action onUpdate) {
        ProcessStartInfo info = ProcessUtils.CreateRedirectedStartInfo("cmd", workingDir);
        info.Arguments = "/c " + settings.Command.Replace("{{PROMPT}}", prompt.Replace("\"", "\\\""));

        bool disposeCanceller = false;
        lock (cancellerLock) {
            if (canceller == null) {
                canceller = new CancellationTokenSource(TimeSpan.FromSeconds(settings.Timeout));
                disposeCanceller = true;
            }
            instance = Process.Start(info) ?? throw new InvalidOperationException("Could not start the agent process.");
        }

        StringBuilder errorOutput = new();
        instance.ErrorDataReceived += (sender, e) => {
            if (e.Data != null) {
                errorOutput.AppendLine(e.Data);
            }
        };
        instance.BeginErrorReadLine();

        DateTime lastUpdate = DateTime.UtcNow;
        TimeSpan updateTimeSpan = TimeSpan.FromMilliseconds(Config.serverPollInterval);
        try {
            Task.Run(async () => {
                while (true) {
                    canceller.Token.ThrowIfCancellationRequested();

                    string line = await instance.StandardOutput.ReadLineAsync(canceller.Token);
                    if (line == null) {
                        break;
                    }

                    output.AppendLine(line);

                    if (DateTime.UtcNow >= lastUpdate + updateTimeSpan) {
                        onUpdate();
                        lastUpdate = DateTime.UtcNow;
                    }
                }
            }, canceller.Token).GetAwaiter().GetResult();
            instance.KillSafe();
        } catch (OperationCanceledException) {
            instance.KillSafe(); // Timeout
        } catch (Exception e) {
            Logger.Error("Error executing agent process: " + e);
            instance.KillSafe();
        } finally {
            if (errorOutput.Length > 0) {
                Logger.Error("Shell error: " + errorOutput.ToString().Trim());
            }

            instance.Dispose();
            instance = null;
            if (disposeCanceller) {
                lock (cancellerLock) {
                    canceller?.Dispose();
                    canceller = null;
                }
            }
        }
    }

    /// <summary>
    /// Maximum length of a reply before truncating to the last chunk. This prevents excessive network usage on very long agent outputs.
    /// </summary>
    public const int uploadLimit = 32768;
}
