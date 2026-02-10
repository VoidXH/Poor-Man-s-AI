using System.Text.Json.Nodes;

using PoorMansAI.Configuration;

namespace PoorMansAI.Engines.Jinja;

/// <summary>
/// A tool where the reply does not need to be transformed, it comes from the model completed.
/// </summary>
public abstract class ReplyBasedTool : Tool {
    /// <summary>
    /// Name of the JSON property that contains the reply.
    /// </summary>
    public abstract string OutputProperty { get; }

    /// <inheritdoc/>
    public sealed override string Execute(LlamaCpp caller, JsonNode parameters, Engine.Progress progressReporter) =>
        parameters[OutputProperty]?.ToString() ?? Config.internalErrorMessage;
}
