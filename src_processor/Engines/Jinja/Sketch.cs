namespace PoorMansAI.Engines.Jinja; 

/// <summary>
/// Makes the LLM draw an SVG file.
/// </summary>
public sealed class Sketch : ReplyBasedTool {
    /// </inheritdoc>
    public override string OutputProperty => "sketch";
}
