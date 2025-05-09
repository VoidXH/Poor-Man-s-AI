using PoorMansAI.Configuration;

namespace PoorMansAI.Engines {
    /// <summary>
    /// Collects all info regarding a large language model.
    /// </summary>
    public class LLModel {
        /// <summary>
        /// Model name on the UI.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Where the model is located on the computer.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// How the LLM should behave.
        /// </summary>
        public string SystemMessage { get; }

        /// <summary>
        /// Put this to the end of all user input.
        /// </summary>
        public string PostMessage { get; }

        /// <summary>
        /// Collects all info regarding a large language model.
        /// </summary>
        /// <param name="prefix">The model is referred to as this in the config file (Model1 and so on)</param>
        /// <param name="large">The file path shall be the large version that runs when the machine only processes LLMs and nothing else</param>
        public LLModel(string prefix, bool large) {
            Dictionary<string, string> config = Config.Values;
            Name = config[prefix];
            FilePath = Path.Combine(Config.models, config[prefix + (large ? "LLM" : "SLM")]);
            SystemMessage = config[prefix + "SystemMessage"];

            string postMessageKey = prefix + "PostMessage";
            if (config.TryGetValue(postMessageKey, out string postMessage)) {
                PostMessage = postMessage;
            }
        }
    }
}