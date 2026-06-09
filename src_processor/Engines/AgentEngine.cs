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
/// <param name="settings">How this instance is configured</param>
public partial class AgentEngine(AgentSettings settings) : Engine {
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
                string result = ExtraCommandHandler(workingDir, extraCommand);
                if (!string.IsNullOrEmpty(result)) {
                    output.AppendLine(result);
                    prompt = prompt[(commandClose + 1)..];
                }
            }
        }
        if (string.IsNullOrEmpty(prompt)) {
            return output.ToString();
        }

        ProcessStartInfo info = ProcessUtils.CreateRedirectedStartInfo("cmd", workingDir);
        info.Arguments = "/c " + settings.Command.Replace("{{PROMPT}}", prompt.Replace("\"", "\\\""));

        lock (cancellerLock) {
            canceller = new CancellationTokenSource(TimeSpan.FromSeconds(settings.Timeout));
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
                while (!canceller.Token.IsCancellationRequested && !instance.HasExited) {
                    string line = await instance.StandardOutput.ReadLineAsync(canceller.Token);
                    if (line == null) {
                        continue;
                    }

                    output.AppendLine(line);
                    if (DateTime.UtcNow >= lastUpdate + updateTimeSpan) {
                        UpdateProgress(command, .5f, output.ToString().Trim());
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
            lock (cancellerLock) {
                canceller?.Dispose();
                canceller = null;
            }
        }

        string reply = output.ToString().Trim();
        if (reply.Length > uploadLimit) {
            string window = reply[^uploadLimit..];
            int split = window.IndexOf('\n');
            if (split != -1) {
                reply = reply[(split + 1)..];
            }
        }
        return reply;
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
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Maximum length of a reply before truncating to the last chunk. This prevents excessive network usage on very long agent outputs.
    /// </summary>
    public const int uploadLimit = 32768;
}
