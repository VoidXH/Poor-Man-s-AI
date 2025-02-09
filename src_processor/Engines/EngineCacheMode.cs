namespace PoorMansAI.Engines {
    /// <summary>
    /// Rules to keep engines alive by.
    /// </summary>
    [Flags]
    public enum EngineCacheMode {
        /// <summary>
        /// No engine runs.
        /// </summary>
        Offline = 0,

        /// <summary>
        /// Have only a single small LM active, leave the GPU free.
        /// </summary>
        SLM = 1,

        /// <summary>
        /// Have only a bigger LLM active, taking up the GPU.
        /// </summary>
        LLM = 2,

        /// <summary>
        /// Have only an image generator active, leave the CPU free.
        /// </summary>
        Image = 4,

        /// <summary>
        /// Have image generation on the GPU and smaller language models on the CPU.
        /// </summary>
        ImageAndSLM = SLM | Image,
    }
}