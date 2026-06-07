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
}
