﻿namespace PoorMansAI.Configuration {
    // Parsed config values related to LLM chatbots
    partial class Config {
        /// <summary>
        /// Where llama.cpp's CPU version is unpacked to.
        /// </summary>
        public static readonly string llamaCppCPURoot = Values["LlamaCppCPURoot"];

        /// <summary>
        /// Where llama.cpp's GPU version is unpacked to.
        /// </summary>
        public static readonly string llamaCppGPURoot = Values["LlamaCppGPURoot"];

        /// <summary>
        /// llama.cpp CPU release build download path.
        /// </summary>
        public static readonly string llamaCppCPUDownload = Values["LlamaCppCPUDownload"];

        /// <summary>
        /// llama.cpp GPU release build download path.
        /// </summary>
        public static readonly string llamaCppGPUDownload = Values["LlamaCppGPUDownload"];

        /// <summary>
        /// llama.cpp CUDA runtime download path (if 404, update build number).
        /// </summary>
        public static readonly string llamaCppCUDADownload = Values["LlamaCppCUDADownload"];

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