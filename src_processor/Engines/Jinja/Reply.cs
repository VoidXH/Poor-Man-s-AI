using PoorMansAI.Configuration;
using System.Text.Json.Nodes;

namespace PoorMansAI.Engines.Jinja {
    /// <summary>
    /// When the model has issues about auto tool usage and wants to use tools for everything, fix it by adding a tool for providing replies.
    /// </summary>
    internal class Reply : Tool {
        /// <inheritdoc/>
        public override string Execute(LlamaCpp caller, JsonNode parameters, Engine.Progress progressReporter) {
            string reply = parameters["reply"]?.ToString();
            if (reply == null) {
                return Config.internalErrorMessage;
            }
            return reply;
        }
    }
}
