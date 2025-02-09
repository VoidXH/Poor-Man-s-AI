﻿namespace PoorMansAI.Configuration {
    /// <summary>
    /// Stores all settings of a Stable Diffusion Model for VoidMoA, parsed from a <paramref name="configKey"/>.
    /// </summary>
    public class ArtistConfiguration(string configKey) {
        /// <summary>
        /// Download path for the checkpoint file.
        /// </summary>
        public readonly string url = Config.Values[configKey];

        /// <summary>
        /// If a keyword is present in the prompt, the artist is selected. Prepared for binary search.
        /// </summary>
        public readonly string[] keywords = Config.ReadKeywordList(configKey + "Keywords");

        /// <summary>
        /// If a selector is present in the prompt, separated by a comma, the artist is selected, and the selector is removed. Prepared for binary search.
        /// </summary>
        public readonly string[] selectors = Config.ReadKeywordList(configKey + "Selectors");

        /// <summary>
        /// Negative prompt, links for negative embedding downloads changed to pt files.
        /// </summary>
        public readonly string negative = ReadNegative(configKey + "Negative");

        /// <summary>
        /// Parse negative prompts, with the URLs of negative embeddings changed to filenames.
        /// </summary>
        internal static string ReadNegative(string key) {
            string[] result = Config.ReadList(key);
            for (int i = 0; i < result.Length; i++) {
                if (result[i].StartsWith("http")) {
                    result[i] = Config.GetPTFilename(result[i]);
                }
            }
            return string.Join(", ", result);
        }
    }
}