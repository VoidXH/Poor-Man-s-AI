namespace PoorMansAI.Configuration {
    // Parsed config values related to LLM chatbots
    partial class Config {
        /// <summary>
        /// The folder where Large Language Models (LLMs) are saved (could be any path, even absolute).
        /// </summary>
        public static readonly string models = Values["Models"];

        /// <summary>
        /// If chat replies are not done in this many seconds, cancel the generation.
        /// </summary>
        public static readonly int textGenTimeout = int.Parse(Values["TextGenTimeout"]);

        /// <summary>
        /// Enumerates the user-set models and returns the prefix for each of them.
        /// </summary
        public static IEnumerable<string> ForEachModel() => ForEach("Model");
    }
}