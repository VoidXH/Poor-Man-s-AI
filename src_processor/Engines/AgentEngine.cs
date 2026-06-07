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
public class AgentEngine(AgentSettings settings) : Engine {
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
        string processCommand = settings.Command.Replace("{{PROMPT}}", command.Prompt.Replace("\"", "\\\""));
        ProcessStartInfo info = new() {
            FileName = "cmd",
            Arguments = $"/c {processCommand}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        StringBuilder output = new();
        DateTime lastUpdate = DateTime.UtcNow;
        TimeSpan updateTimeSpan = TimeSpan.FromMilliseconds(Config.serverPollInterval);

        lock (cancellerLock) {
            canceller = new CancellationTokenSource(TimeSpan.FromSeconds(settings.Timeout));
            instance = Process.Start(info) ?? throw new InvalidOperationException("Could not start the agent process.");
        }

        try {
            Task.Run(async () => {
                while (!canceller.Token.IsCancellationRequested) {
                    string line = await instance.StandardOutput.ReadLineAsync(canceller.Token);
                    if (line == null) {
                        break; // End of stream
                    }

                    output.AppendLine(line);
                    if (DateTime.UtcNow >= lastUpdate + updateTimeSpan) {
                        UpdateProgress(command, .5f, output.ToString().Trim());
                        lastUpdate = DateTime.UtcNow;
                    }
                }
            }, canceller.Token).GetAwaiter().GetResult();
            instance.WaitForExit();
        } catch (OperationCanceledException) {
            instance.KillSafe(); // Timeout
        } catch (Exception e) {
            Logger.Error("Error executing agent process: " + e);
            instance.KillSafe();
        } finally {
            instance.Dispose();
            instance = null;
            lock (cancellerLock) {
                canceller?.Dispose();
                canceller = null;
            }
        }

        return output.ToString().Trim();
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
}
