using PoorMansAI.Engines.Models;

namespace PoorMansAI.Configuration;

// Parsed config values related to remote agentic tool control
partial class Config {
    /// <summary>
    /// If agent replies are not done in this many seconds, cancel the generation.
    /// </summary>
    public static readonly int agentTimeout = int.Parse(GetValue("AgentTimeout"));

    /// <summary>
    /// Whether to enable sending emails from the agent.
    /// </summary>
    public static readonly bool agentSendEmail = bool.Parse(GetValue("AgentSendEmail"));

    /// <summary>
    /// SMTP server address for sending emails.
    /// </summary>
    public static readonly string agentEmailServer = GetValue("AgentEmailServer");

    /// <summary>
    /// SMTP port (587 for TLS, 465 for SSL, 25 for unencrypted).
    /// </summary>
    public static readonly int agentSmtpPort = int.Parse(GetValue("AgentSMTPPort"));

    /// <summary>
    /// Whether to use STARTTLS for the SMTP connection.
    /// </summary>
    public static readonly bool agentSmtpStartTLS = bool.Parse(GetValue("AgentSMTPStartTLS"));

    /// <summary>
    /// Email account username for SMTP authentication.
    /// </summary>
    public static readonly string agentEmailUser = GetValue("AgentEmailUser");

    /// <summary>
    /// Email account password for SMTP authentication.
    /// </summary>
    public static readonly string agentEmailPassword = GetValue("AgentEmailPassword");

    /// <summary>
    /// Email recipient for agent completion notifications.
    /// </summary>
    public static readonly string agentEmailRecipient = GetValue("AgentEmailRecipient");

    /// <summary>
    /// Interval in seconds between agent progress updates sent to the server.
    /// </summary>
    public static readonly int agentUpdateInterval = int.Parse(GetValue("AgentUpdateInterval"));

    /// <summary>
    /// Comma-separated list of folder paths that the agent is allowed to use as working directory.
    /// If set, any working directory whose path does not start with one of these entries is rejected.
    /// </summary>
    public static readonly string[] agentFolderWhitelist = GetValue("AgentFolderWhitelist").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// Remove disabled tools lines from agent output.
    /// </summary>
    public static readonly bool agentSanitization = bool.Parse(GetValue("AgentSanitization"));

    /// <summary>
    /// Parse and return all configured agent settings (Agent1*, Agent2*, etc.).
    /// </summary>
    public static IEnumerable<AgentSettings> GetAllAgentSettings() {
        foreach (string agentPrefix in ForEachAgent()) {
            yield return new AgentSettings(agentPrefix);
        }
    }
}
