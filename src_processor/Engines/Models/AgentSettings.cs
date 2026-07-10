using PoorMansAI.Configuration;

namespace PoorMansAI.Engines.Models;

/// <summary>
/// All settings for an agent engine with their default values.
/// </summary>
public class AgentSettings {
    /// <summary>
    /// The command template to run. Replace {{PROMPT}} with the user's input.
    /// </summary>
    public string Command { get; set; } = "copilot -p \"{{PROMPT}}\"";

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
    /// API key for the model provider, if the provider requires authentication.
    /// </summary>
    public string CopilotProviderApiKey { get; set; }

    /// <summary>
    /// Maximum input/output tokens for the agent model. For BYOM, at the time of implementation, both are the context size.
    /// </summary>
    public int CopilotMaxTokens { get; set; } = 4096;

    /// <summary>
    /// Read settings for a specific numbered agent (e.g. Agent1Command).
    /// </summary>
    /// <param name="agentPrefix">The agent prefix, e.g. "Agent1".</param>
    public AgentSettings(string agentPrefix) {
        Command = Config.GetValue(agentPrefix + "Command");
        Config.TryGetValue(agentPrefix + "CopilotModel", out string copilotModel);
        CopilotModel = copilotModel;
        Config.TryGetValue(agentPrefix + "CopilotOffline", out string copilotOffline);
        CopilotOffline = copilotOffline;
        Config.TryGetValue(agentPrefix + "CopilotProviderBaseUrl", out string copilotProviderBaseUrl);
        CopilotProviderBaseUrl = copilotProviderBaseUrl;
        Config.TryGetValue(agentPrefix + "CopilotProviderApiKey", out string copilotProviderApiKey);
        CopilotProviderApiKey = copilotProviderApiKey;
        if (Config.TryGetValue(agentPrefix + "CopilotMaxTokens", out string copilotMaxTokens) && int.TryParse(copilotMaxTokens, out int maxTokens)) {
            CopilotMaxTokens = maxTokens;
        }
    }

    /// <summary>
    /// Get the agent list from the main configuration file.
    /// </summary>
    /// <returns>A dictionary mapping each agent's name to its settings.</returns>
    public static Dictionary<string, AgentModel> GetConfiguredAgents() {
        Dictionary<string, AgentModel> agents = [];
        foreach (string prefix in Config.ForEachAgent()) {
            AgentModel agent = new(prefix);
            agents[agent.Name] = agent;
        }
        return agents;
    }
}
