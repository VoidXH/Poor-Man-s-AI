namespace PoorMansAI.Engines.Models;

/// <summary>
/// All settings for the llama.cpp engine with their default values.
/// </summary>
public class LlamaCppSettings {
    /// <summary>
    /// Open the HTTP endpoints on this port.
    /// </summary>
    public ushort Port { get; set; } = 64100;

    /// <summary>
    /// If chat replies are not done in this many seconds, cancel the generation.
    /// </summary>
    public int Timeout { get; set; } = 90;

    /// <summary>
    /// When switching models, allow this many extra seconds over the normal timeout.
    /// </summary>
    public int Loading { get; set; } = 15;

    /// <summary>
    /// Maximum context length for prompts.
    /// </summary>
    public int Context { get; set; } = 4096;

    /// <summary>
    /// How much to keep of the initial prompt context for each generation.
    /// </summary>
    public int Keep { get; set; } = 128;

    /// <summary>
    /// How much space to keep at the end of the context window for new messages.
    /// </summary>
    public int Discard { get; set; } = 512;
}
