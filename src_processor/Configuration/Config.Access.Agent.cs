namespace PoorMansAI.Configuration;

// Parsed config values related to remote agentic tool control
partial class Config {
    /// <summary>
    /// Command template for the agent engine. Replace {{PROMPT}} with the user's input.
    /// </summary>
    public static readonly string agentCommand = Values["AgentCommand"];

    /// <summary>
    /// If agent replies are not done in this many seconds, cancel the generation.
    /// </summary>
    public static readonly int agentTimeout = int.Parse(Values["AgentTimeout"]);

    /// <summary>
    /// Whether to enable sending emails from the agent.
    /// </summary>
    public static readonly bool agentSendEmail = bool.Parse(Values["AgentSendEmail"]);

    /// <summary>
    /// SMTP server address for sending emails.
    /// </summary>
    public static readonly string agentEmailServer = Values["AgentEmailServer"];

    /// <summary>
    /// SMTP port (587 for TLS, 465 for SSL, 25 for unencrypted).
    /// </summary>
    public static readonly int agentSmtpPort = int.Parse(Values["AgentSMTPPort"]);

    /// <summary>
    /// Whether to use STARTTLS for the SMTP connection.
    /// </summary>
    public static readonly bool agentSmtpStartTLS = bool.Parse(Values["AgentSMTPStartTLS"]);

    /// <summary>
    /// Email account username for SMTP authentication.
    /// </summary>
    public static readonly string agentEmailUser = Values["AgentEmailUser"];

    /// <summary>
    /// Email account password for SMTP authentication.
    /// </summary>
    public static readonly string agentEmailPassword = Values["AgentEmailPassword"];

    /// <summary>
    /// Email recipient for agent completion notifications.
    /// </summary>
    public static readonly string agentEmailRecipient = Values["AgentEmailRecipient"];

    /// <summary>
    /// Interval in seconds between agent progress updates sent to the server.
    /// </summary>
    public static readonly int agentUpdateInterval = int.Parse(Values["AgentUpdateInterval"]);

    /// <summary>
    /// Comma-separated list of folder paths that the agent is allowed to use as working directory.
    /// If set, any working directory whose path does not start with one of these entries is rejected.
    /// </summary>
    public static readonly string[] agentFolderWhitelist = Values["AgentFolderWhitelist"].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// How GitHub Copilot (if used as agent) will display the model.
    /// </summary>
    public static readonly string agentCopilotModel = Values["AgentCopilotModel"];

    /// <summary>
    /// GitHub Copilot (if used as agent) shall not connect to GitHub at all.
    /// </summary>
    public static readonly string agentCopilotOffline = Values["AgentCopilotOffline"];

    /// <summary>
    /// Where your chat model is ran at if GitHub Copilot is used as agent.
    /// </summary>
    public static readonly string agentCopilotProviderBaseUrl = Values["AgentCopilotProviderBaseUrl"];

    /// <summary>
    /// Maximum input/output tokens for the agent model. For BYOM, at the time of implementation, both are the context size.
    /// </summary>
    public static readonly int agentCopilotMaxTokens = int.Parse(Values["AgentCopilotMaxTokens"]);
}
