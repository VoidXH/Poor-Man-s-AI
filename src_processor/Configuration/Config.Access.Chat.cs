namespace PoorMansAI.Configuration {
    // Parsed config values related to LLM chatbots
    partial class Config {
        /// <summary>
        /// Where llama.cpp's CPU version is unpacked to.
        /// </summary>
        public static readonly string llamaCppCPURoot = GetValue("LlamaCppCPURoot");

        /// <summary>
        /// Where llama.cpp's GPU version is unpacked to.
        /// </summary>
        public static readonly string llamaCppGPURoot = GetValue("LlamaCppGPURoot");

        /// <summary>
        /// Folder of extra knowledge for the chat.
        /// </summary>
        public static readonly string contextDocTree = GetValue("ContextDocTree");

        /// <summary>
        /// Load all loosely matching context docs.
        /// </summary>
        public static readonly bool fullContext = bool.Parse(GetValue("FullContext"));

        /// <summary>
        /// Only search context docs for the latest prompt.
        /// </summary>
        public static readonly bool augmentLatestOnly = bool.Parse(GetValue("AugmentLatestOnly"));

        /// <summary>
        /// Also add the system prompt to each augmentation evaluation.
        /// </summary>
        public static readonly bool augmentWithSystemPrompt = bool.Parse(GetValue("AugmentWithSystemPrompt"));

        /// <summary>
        /// llama.cpp CPU release build download path.
        /// </summary>
        public static readonly string llamaCppCPUDownload = GetValue("LlamaCppCPUDownload");

        /// <summary>
        /// llama.cpp GPU release build download path.
        /// </summary>
        public static readonly string llamaCppGPUDownload = GetValue("LlamaCppGPUDownload");

        /// <summary>
        /// llama.cpp CUDA runtime download path (if 404, update build number).
        /// </summary>
        public static readonly string llamaCppCUDADownload = GetValue("LlamaCppCUDADownload");

        /// <summary>
        /// The folder where Large Language Models (LLMs) are saved (could be any path, even absolute).
        /// </summary>
        public static readonly string models = Path.GetFullPath(GetValue("Models"));

        /// <summary>
        /// Port used by the main LLM runner engine.
        /// </summary>
        public static readonly ushort llamaCppPort = ushort.Parse(GetValue("LlamaCppPort"));

        /// <summary>
        /// Allow local network to reach the chat's native webpage.
        /// </summary>
        public static readonly bool chatLocalhost = bool.Parse(GetValue("ChatLocalhost"));

        /// <summary>
        /// If chat replies are not done in this many seconds, cancel the generation.
        /// </summary>
        public static readonly int chatTimeout = int.Parse(GetValue("ChatTimeout"));

        /// <summary>
        /// When switching models, allow this many extra seconds over the normal timeout.
        /// </summary>
        public static readonly int chatLoading = int.Parse(GetValue("ChatLoading"));

        /// <summary>
        /// Maximum number of tokens to generate (-1 = infinite).
        /// </summary>
        public static readonly int chatPredict = int.Parse(GetValue("ChatPredict"));

        /// <summary>
        /// Maximum context length for prompts when small models are in use.
        /// </summary>
        public static readonly int chatContextSLM = int.Parse(GetValue("ChatContextSLM"));

        /// <summary>
        /// Maximum context length for prompts when large models are in use.
        /// </summary>
        public static readonly int chatContextLLM = int.Parse(GetValue("ChatContextLLM"));

        /// <summary>
        /// Number of tokens to maximally use for a single reasoning session.
        /// </summary>
        public static readonly int chatReasoningBudget = int.Parse(GetValue("ChatReasoningBudget"));

        /// <summary>
        /// How much to keep of the initial prompt context for each generation.
        /// </summary>
        public static readonly int chatKeep = int.Parse(GetValue("ChatKeep"));

        /// <summary>
        /// How much space to keep at the end of the context window for new messages.
        /// </summary>
        public static readonly int chatDiscard = int.Parse(GetValue("ChatDiscard"));

        /// <summary>
        /// When using models with Multi-Token Prediction, predict this many at once.
        /// </summary>
        public static readonly int chatMTP = int.Parse(GetValue("ChatMTP"));

        /// <summary>
        /// Parallel requests chat can serve, larger values take up more RAM.
        /// </summary>
        public static readonly int chatParallel = int.Parse(GetValue("ChatParallel"));

        /// <summary>
        /// Apply PMAI-supplied fixes to some known wrong LLM templates.
        /// </summary>
        public static readonly bool chatFixes = bool.Parse(GetValue("ChatFixes"));

        /// <summary>
        /// Enumerate the names of configured models.
        /// </summary>
        public static IEnumerable<string> GetModelNames() => ForEachModel().Select(x => GetValue(x));

        /// <summary>
        /// Enumerates the user-set models and returns the prefix for each of them.
        /// </summary
        internal static IEnumerable<string> ForEachModel() => ForEach("Model");
    }
}