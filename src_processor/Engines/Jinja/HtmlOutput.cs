using System.Text.Json.Nodes;

using PoorMansAI.Engines.BaseClasses;
using PoorMansAI.Engines.Jinja.BaseClasses;

namespace PoorMansAI.Engines.Jinja;

/// <summary>
/// Outputs raw HTML directly as the tool response. The LLM can use this to produce formatted HTML content.
/// </summary>
public sealed class HtmlOutput : ReplyBasedTool {
    /// <inheritdoc/>
    public override string OutputProperty => "html";

    /// <inheritdoc/>
    public override string Execute(LlamaCpp caller, JsonNode parameters, Command command, Engine.Progress progressReporter) => parameters[OutputProperty]?.ToString();
}
