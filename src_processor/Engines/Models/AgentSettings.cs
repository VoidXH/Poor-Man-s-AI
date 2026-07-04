using PoorMansAI.Configuration;

namespace PoorMansAI.Engines.Models;

/// <summary>
/// All settings for the agent engine with their default values.
/// </summary>
public class AgentSettings {
    /// <summary>
    /// If chat replies are not done in this many seconds, cancel the generation.
    /// </summary>
    public int Timeout { get; set; } = 600;

    /// <summary>
    /// The command template to run. Replace {{PROMPT}} with the user's input.
    /// </summary>
    public string Command { get; set; } = "copilot -p \"{{PROMPT}}\"";

    /// <summary>
    /// Interval in seconds between agent progress updates sent to the server.
    /// </summary>
    public int UpdateInterval { get; set; } = 10;

    /// <summary>
    /// Comma-separated list of folder paths that the agent is allowed to use as working directory.
    /// If set, any working directory whose path does not start with one of these entries is rejected.
    /// </summary>
    public string[] FolderWhitelist { get; set; } = [];

    /// <summary>
    /// How GitHub Copilot (if used as agent) will display the model.
    /// </summary>
    public string CopilotModel { get; set; } = "Poor Man's AI";

    /// <summary>
    /// GitHub Copilot (if used as agent) shall not connect to GitHub at all.
    /// </summary>
    public string CopilotOffline { get; set; } = "true";

    /// <summary>
    /// Where your chat model is ran at if GitHub Copilot is used as agent.
    /// </summary>
    public string CopilotProviderBaseUrl { get; set; } = "http://localhost:64100/v1";

    /// <summary>
    /// Maximum input/output tokens for the agent model. For BYOM, at the time of implementation, both are the context size.
    /// </summary>
    public int CopilotMaxTokens { get; set; } = 4096;

    /// <summary>
    /// Create a settings holder with default values or read the settings for the agent engine from the main configuration file.
    /// </summary>
    public AgentSettings(bool readConfig) {
        if (readConfig) {
            Timeout = Config.agentTimeout;
            Command = Config.agentCommand;
            UpdateInterval = Config.agentUpdateInterval;
            FolderWhitelist = Config.agentFolderWhitelist;
            CopilotModel = Config.agentCopilotModel;
            CopilotOffline = Config.agentCopilotOffline;
            CopilotProviderBaseUrl = Config.agentCopilotProviderBaseUrl;
            CopilotMaxTokens = Config.agentCopilotMaxTokens;
        }
    }
}
