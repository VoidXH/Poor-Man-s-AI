using PoorMansAI.Engines.BaseClasses;
using System.Text.Json.Nodes;

namespace PoorMansAI.Engines.Jinja.BaseClasses {
    /// <summary>
    /// A tool which can wrap its reply between a prefix and suffix text.
    /// Tool configurations must have a &quot;prefix&quot; and &quot;suffix&quot; property described for this use.
    /// </summary>
    public abstract class SandwichTool : ReplyBasedTool {
        /// <inheritdoc/>
        public override string Execute(LlamaCpp caller, JsonNode parameters, Command command, Engine.Progress progressReporter) {
            JsonNode prefix = parameters["prefix"];
            string prefixOut = string.Empty;
            if (prefix != null) {
                prefixOut = prefix.ToString() + "<br>";
            }

            JsonNode suffix = parameters["suffix"];
            string suffixOut = string.Empty;
            if (suffix != null) {
                suffixOut = "<br>" + suffix.ToString();
            }

            return prefixOut + base.Execute(caller, parameters, command, progressReporter) + suffixOut;
        }
    }
}
