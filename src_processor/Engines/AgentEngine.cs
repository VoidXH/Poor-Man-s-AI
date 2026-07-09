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
    /// Each selectable agent with its key from the website.
    /// </summary>
    readonly Dictionary<string, AgentModel> agents;

    /// <summary>
    /// The default agent used when no agent is specified in the command.
    /// </summary>
    readonly AgentModel defaultAgent;

    /// <summary>
    /// Safely handles the <see cref="promptCancellers"/> dictionary from multiple threads.
    /// </summary>
    readonly object cancellersLock = new();

    /// <summary>
    /// Maps each active prompt ID to its cancellation token source. Removed automatically when the prompt finishes or fails.
    /// </summary>
    readonly Dictionary<int, CancellationTokenSource> promptCancellers = [];

    /// <summary>
    /// Tracks the currently running prompt ID so that <see cref="StopGeneration"/> only cancels that specific prompt.
    /// </summary>
    volatile int currentPromptId = -1;

    /// <summary>
    /// LLM chatbot running an external CLI agent per prompt, loading agents from the root configuration file.
    /// </summary>
    /// <param name="settings">How this instance is configured</param>
    public AgentEngine() : this(AgentSettings.GetConfiguredAgents()) { }

    /// <summary>
    /// LLM chatbot running an external CLI agent per prompt.
    /// </summary>
    /// <param name="agents">Each agent with its key from the website</param>
    public AgentEngine(Dictionary<string, AgentModel> agents) {
        this.agents = agents;
        defaultAgent = agents.First().Value;
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
        string prompt = command.Prompt;

        if (prompt.StartsWith("[Queue:") && prompt[^1] == ']') {
            return ExtraCommandHandler(string.Empty, prompt[1..^1], command.ID);
        }

        AgentModel selectedAgent = defaultAgent;
        if (prompt.StartsWith('<')) {
            int closeBracket = prompt.IndexOf('>');
            if (closeBracket > 0) {
                string agentName = prompt[1..closeBracket].Trim();
                if (agents.TryGetValue(agentName, out AgentModel agent)) {
                    selectedAgent = agent;
                    prompt = prompt[(closeBracket + 1)..].Trim();
                }
            }
        }

        string workingDir = string.Empty;
        int splitter = prompt.IndexOf('|');
        if (splitter >= 0) {
            workingDir = prompt[..splitter].Trim();
            prompt = prompt[(splitter + 1)..].Trim();
        }

        if (string.IsNullOrEmpty(workingDir) || !Directory.Exists(workingDir)) {
            workingDir = Environment.CurrentDirectory;
        }

        if (Config.agentFolderWhitelist.Length > 0) {
            bool allowed = false;
            foreach (string whitelistEntry in Config.agentFolderWhitelist) {
                if (workingDir.StartsWith(whitelistEntry, StringComparison.OrdinalIgnoreCase)) {
                    allowed = true;
                    break;
                }
            }
            if (!allowed) {
                Logger.Warning($"Agent execution path \"{workingDir}\" is not in the folder whitelist. Request rejected.");
                return null;
            }
        }

        StringBuilder output = new();
        while (prompt.StartsWith('[')) {
            int commandClose = FindClosingBracket(prompt);
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

        Logger.Info("Agent command processing started.");
        Stopwatch stopwatch = Stopwatch.StartNew();
        RunAgentEngine(workingDir, command.ID, prompt, output, selectedAgent, () => UpdateProgress(command, .5f, CutOutput(output.ToString())));
        stopwatch.Stop();
        string fulloutput = output.ToString().Trim();
        string finalOutput = CutOutput(fulloutput);

        if (Config.agentSendEmail) {
            try {
                EmailSender.Send(Config.agentEmailRecipient, "Agent Process Completed",
                    $"Prompt: {prompt}\n\nProject folder: {workingDir}\n\nAgent time: {stopwatch.Elapsed:mm\\:ss}\n\n{fulloutput}");
            } catch (Exception) {
                // Email sending failure is non-fatal; ignore silently
            }
        }

        Logger.Info("Agent command processed.");
        return string.IsNullOrEmpty(finalOutput)
            ? "Error."
            : finalOutput;
    }

    /// <inheritdoc/>
    public override void StopGeneration() {
        int promptId = currentPromptId;
        if (promptId == -1) {
            return;
        }
        lock (cancellersLock) {
            if (promptCancellers.TryGetValue(promptId, out CancellationTokenSource ct)) {
                ct.Cancel();
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
    void RunAgentEngine(string workingDir, int promptId, string prompt, StringBuilder output, AgentModel agent, Action onUpdate) {
        ProcessStartInfo info = ProcessUtils.CreateRedirectedStartInfo("cmd", workingDir);
        info.Arguments = "/c " + agent.Command.Replace("{{PROMPT}}", prompt.Replace("\"", "\\\""));

        if (!string.IsNullOrEmpty(agent.CopilotModel)) {
            info.Environment["COPILOT_MODEL"] = agent.CopilotModel;
        }
        if (!string.IsNullOrEmpty(agent.CopilotOffline)) {
            info.Environment["COPILOT_OFFLINE"] = agent.CopilotOffline;
        }
        if (!string.IsNullOrEmpty(agent.CopilotProviderBaseUrl)) {
            info.Environment["COPILOT_PROVIDER_BASE_URL"] = agent.CopilotProviderBaseUrl;
        }
        if (!string.IsNullOrEmpty(agent.CopilotProviderApiKey)) {
            info.Environment["COPILOT_PROVIDER_API_KEY"] = agent.CopilotProviderApiKey;
        }
        if (agent.CopilotMaxTokens.HasValue) {
            string contextWindow = agent.CopilotMaxTokens.Value.ToString();
            info.Environment["COPILOT_PROVIDER_MAX_OUTPUT_TOKENS"] = contextWindow;
            info.Environment["COPILOT_PROVIDER_MAX_PROMPT_TOKENS"] = contextWindow;
        }

        Process instance;
        CancellationTokenSource localCanceller = new(TimeSpan.FromSeconds(Config.agentTimeout));

        if (promptId != -1) {
            currentPromptId = promptId;
        }
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
        TimeSpan updateTimeSpan = TimeSpan.FromSeconds(Config.agentUpdateInterval);

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
            if (currentPromptId == promptId) {
                currentPromptId = -1;
            }
        }
    }

    /// <summary>
    /// Finds the index of the closing bracket ']' that matches the opening bracket at position 0.
    /// Counts internal bracket depth so that brackets inside the command text don't
    /// cause premature termination. Returns -1 if no matching closing bracket is found.
    /// </summary>
    static int FindClosingBracket(string prompt) {
        int depth = 0;
        for (int i = 0; i < prompt.Length; i++) {
            char c = prompt[i];
            if (c == '[') {
                depth++;
            } else if (c == ']') {
                depth--;
                if (depth == 0) {
                    return i;
                }
            }
        }
        return -1;
    }

    /// <summary>
    /// Maximum length of a reply before truncating to the last chunk. This prevents excessive network usage on very long agent outputs.
    /// </summary>
    public const int uploadLimit = 32768;
}
