using PoorMansAI.Engines.BaseClasses;
using System.Text.Json.Nodes;

namespace PoorMansAI.Engines.Jinja.BaseClasses {
    /// <summary>
    /// A tool which can wrap its reply between a prefix and suffix text.
    /// Tool configurations must have a &quot;prefix&quot; and &quot;suffix&quot; property described for this use.
    /// </summary>
    public abstract class SandwichTool : ReplyBasedTool {
        /// <inheritdoc/>
        public sealed override string Execute(LlamaCpp caller, JsonNode parameters, Engine.Progress progressReporter) =>
            parameters["prefix"]?.ToString() ?? string.Empty +
            base.Execute(caller, parameters, progressReporter) +
            parameters["suffix"]?.ToString() ?? string.Empty;
    }
}
