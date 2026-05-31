using PoorMansAI.Engines.Jinja.BaseClasses;

namespace PoorMansAI.Engines.Jinja;

/// <summary>
/// Makes the LLM draw an SVG file.
/// </summary>
public sealed class Sketch : SandwichTool {
    /// </inheritdoc>
    public override string OutputProperty => "sketch";
}
