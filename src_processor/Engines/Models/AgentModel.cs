using PoorMansAI.Configuration;

namespace PoorMansAI.Engines.Models;

/// <summary>
/// Collects all info regarding an agent engine.
/// </summary>
public class AgentModel {
    /// <summary>
    /// Agent name on the UI.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The command template to run. Replace {{PROMPT}} with the user's input.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// How GitHub Copilot (if used as agent) will display the model. Optional.
    /// </summary>
    public string CopilotModel { get; set; }

    /// <summary>
    /// GitHub Copilot (if used as agent) shall not connect to GitHub at all. Optional.
    /// </summary>
    public string CopilotOffline { get; set; }

    /// <summary>
    /// Where your chat model is ran at if GitHub Copilot is used as agent. Optional.
    /// </summary>
    public string CopilotProviderBaseUrl { get; set; }

    /// <summary>
    /// The API key for the Copilot-compatible provider (for example OpenRouter). Optional.
    /// </summary>
    public string CopilotProviderApiKey { get; set; }

    /// <summary>
    /// Maximum input/output tokens for the agent model. For BYOM, at the time of implementation, both are the context size. Optional.
    /// </summary>
    public int? CopilotMaxTokens { get; set; }

    /// <summary>
    /// Collects all info regarding an agent engine.
    /// </summary>
    /// <param name="prefix">The agent is referred to as this in the config file (Agent1 and so on)</param>
    public AgentModel(string prefix) {
        Name = Config.GetValue(prefix);
        Command = Config.GetValue(prefix + "Command");
        Config.TryGetValue(prefix + "CopilotModel", out string copilotModel);
        CopilotModel = copilotModel;
        Config.TryGetValue(prefix + "CopilotOffline", out string copilotOffline);
        CopilotOffline = copilotOffline;
        Config.TryGetValue(prefix + "CopilotProviderBaseUrl", out string copilotProviderBaseUrl);
        CopilotProviderBaseUrl = copilotProviderBaseUrl;
        Config.TryGetValue(prefix + "CopilotProviderApiKey", out string copilotProviderApiKey);
        CopilotProviderApiKey = copilotProviderApiKey;
        if (Config.TryGetValue(prefix + "CopilotMaxTokens", out string copilotMaxTokens) && int.TryParse(copilotMaxTokens, out int maxTokens)) {
            CopilotMaxTokens = maxTokens;
        }
    }
}
