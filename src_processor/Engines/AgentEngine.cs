using System.Diagnostics;
using System.Text;

using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Engines.BaseClasses;
using PoorMansAI.Engines.Models;
using PoorMansAI.Engines.Utilities;
using PoorMansAI.Services;

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
    /// Safely handles the <see cref="promptCancellers"/> dictionary from multiple threads.
    /// </summary>
    readonly object cancellersLock = new();

    /// <summary>
    /// Maps each active prompt ID to its cancellation token source. Removed automatically when the prompt finishes or fails.
    /// </summary>
    readonly Dictionary<int, CancellationTokenSource> promptCancellers = [];

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
        Logger.Info("Agent command processing started.");
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
        while (prompt.StartsWith('[')) {
            int commandClose = prompt.IndexOf(']', StringComparison.Ordinal);
            if (commandClose < 0) {
                break;
            }

            string extraCommand = prompt[1..commandClose];
            string result = ExtraCommandHandler(workingDir, extraCommand, command.ID);
            if (!string.IsNullOrEmpty(result)) {
                output.AppendLine(result);
            }
            prompt = prompt[(commandClose + 1)..];
        }

        string preprocessing = output.ToString();
        if (string.IsNullOrEmpty(prompt)) {
            return preprocessing;
        }

        if (!string.IsNullOrEmpty(preprocessing)) {
            output.Clear();
            prompt = $"{preprocessing.Replace("\"", "\\\"")}\n{prompt}";
        }

        RunAgentEngine(workingDir, command.ID, prompt, output, () => UpdateProgress(command, .5f, CutOutput(output.ToString())));
        string fulloutput = output.ToString().Trim();
        string finalOutput = CutOutput(fulloutput);

        if (Config.agentSendEmail) {
            try {
                EmailSender.Send(Config.agentEmailRecipient, "Agent Process Completed", $"Prompt: {prompt}\n\nProject folder: {workingDir}\n\n{fulloutput}");
            } catch (Exception) {
                // Email sending failure is non-fatal; ignore silently
            }
        }

        Logger.Info("Agent command processed.");
        return string.IsNullOrEmpty(finalOutput) ?
             "Error." :
             finalOutput;
    }

    /// <inheritdoc/>
    public override void StopGeneration() {
        lock (cancellersLock) {
            foreach (KeyValuePair<int, CancellationTokenSource> pair in promptCancellers) {
                pair.Value.Cancel();
            }
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
    void RunAgentEngine(string workingDir, int promptId, string prompt, StringBuilder output, Action onUpdate) {
        ProcessStartInfo info = ProcessUtils.CreateRedirectedStartInfo("cmd", workingDir);
        info.Arguments = "/c " + settings.Command.Replace("{{PROMPT}}", prompt.Replace("\"", "\\\""));

        Process instance;
        CancellationTokenSource localCanceller = new(TimeSpan.FromSeconds(settings.Timeout));

        lock (cancellersLock) {
            promptCancellers[promptId] = localCanceller;
            instance = Process.Start(info) ?? throw new InvalidOperationException("Could not start the agent process.");
        }

        StringBuilder errorOutput = new();
        instance.ErrorDataReceived += (sender, e) => {
            if (e.Data != null) {
                errorOutput.AppendLine(e.Data);
            }
        };
        instance.BeginErrorReadLine();

        DateTime lastUpdate = DateTime.Today - TimeSpan.FromDays(1); // Force an update on the first line
        TimeSpan updateTimeSpan = TimeSpan.FromSeconds(settings.UpdateInterval);

        try {
            Task.Run(async () => {
                Task<string> readTask = null;
                while (true) {
                    localCanceller.Token.ThrowIfCancellationRequested();

                    readTask ??= instance.StandardOutput.ReadLineAsync();
                    Task completed = await Task.WhenAny(readTask, Task.Delay(updateTimeSpan, localCanceller.Token));

                    if (completed == readTask) {
                        string line = await readTask;
                        if (line == null) {
                            break;
                        }

                        output.AppendLine(line);
                        readTask = null;
                    }

                    onUpdate(); // Heartbeat even if nothing was read
                }
            }, localCanceller.Token).GetAwaiter().GetResult();
            instance.KillSafe();
        } catch (OperationCanceledException) {
            try { // Drain lines still in the standard output
                while (true) {
                    instance.StandardOutput.BaseStream.ReadTimeout = 100;
                    string line = instance.StandardOutput.ReadLine();
                    if (line == null) {
                        break;
                    }
                    output.AppendLine(line);
                }
            } catch { }
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

            lock (cancellersLock) {
                if (promptCancellers.TryGetValue(promptId, out CancellationTokenSource ct)) {
                    if (ReferenceEquals(ct, localCanceller)) {
                        promptCancellers.Remove(promptId);
                        ct.Dispose();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Maximum length of a reply before truncating to the last chunk. This prevents excessive network usage on very long agent outputs.
    /// </summary>
    public const int uploadLimit = 32768;
}
