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
    /// Create a settings holder with default values or read the settings for the agent engine from the main configuration file.
    /// </summary>
    public AgentSettings(bool readConfig) {
        if (readConfig) {
            Timeout = Config.agentTimeout;
            Command = Config.agentCommand;
            UpdateInterval = Config.agentUpdateInterval;
        }
    }
}
