using System.Text.Json.Nodes;

namespace PoorMansAI.Engines.Utilities {
    /// <summary>
    /// Parses the JSON reasoning_content to the legacy content formatting with the &lt;think&gt; tag.
    /// </summary>
    public class ReasoningParser {
        /// <summary>
        /// We are currently in a reasoning block.
        /// </summary>
        bool reasoning;

        /// <summary>
        /// Process a <paramref name="delta"/> block that contains either content or reasoning_content.
        /// </summary>
        public string Process(JsonNode delta) {
            JsonNode reasoningContent = delta["reasoning_content"];
            if (reasoningContent != null) {
                if (!reasoning) {
                    reasoning = true;
                    return "<think>" + reasoningContent.ToString();
                }
                return reasoningContent.ToString();
            } else {
                JsonNode content = delta["content"];
                if (reasoning) {
                    reasoning = false;
                    return "</think>" + content.ToString();
                }
                return content.ToString();
            }
        }
    }
}
