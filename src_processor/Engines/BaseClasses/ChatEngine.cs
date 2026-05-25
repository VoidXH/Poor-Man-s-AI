namespace PoorMansAI.Engines.BaseClasses;

/// <summary>
/// An engine implementing text-only LLM interaction.
/// </summary>
public abstract class ChatEngine : Engine {
    /// <summary>
    /// Whether this engine's model runs on the GPU.
    /// </summary>
    public abstract bool GPU { get; }
}