﻿using PoorMansAI.Configuration;

namespace PoorMansAI.NewTech.VoidMoA {
    /// <summary>
    /// Extracts suggested model and negative prompt from a general prompt.
    /// </summary>
    public static partial class PromptTransformer {
        /// <summary>
        /// Extracts suggested model and negative prompt from a general <paramref name="prompt"/>.
        /// </summary>
        public static TransformedPrompt Transform(string prompt) {
            TransformedPrompt result = new();
            const string base64Header = "data:image/png;base64,";
            int pipe = prompt.IndexOf('|');
            if (prompt.StartsWith(base64Header) && pipe != -1) {
                result.Endpoint = "img2img";
                result.ReferenceImages = [prompt[base64Header.Length..pipe]];
                prompt = prompt[(pipe + 1)..];
            } else {
                result.Endpoint = "txt2img";
            }

            // Every single word
            string[] keywords = [.. prompt.Split(keywordSplits, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLowerInvariant())];
            // Potential resolution or other property selectors
            string[] selectors = [.. prompt.Split(selectorSplits, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLowerInvariant())];

            ArtistConfiguration[] artists = Config.artistConfigs;
            int[] votes = new int[artists.Length];
            int maxVotes = 0,
                maxVotesFor = 0;
            for (int i = 0; i < artists.Length; i++) {
                votes[i] = HasSelector(ref selectors, artists[i].selectors) + HasKeyword(keywords, artists[i].keywords);
                if (maxVotes < votes[i]) {
                    maxVotes = votes[i];
                    maxVotesFor = i;
                }
            }
            result.Model = Config.GetSafetensorFilenameWithoutExtension(maxVotes == 0 ? Config.defaultArtist : artists[maxVotesFor].url);
            result.NegativePrompt = maxVotes == 0 ? Config.defaultNegative : artists[maxVotesFor].negative;

            int widescreen = HasSelector(ref selectors, Config.hSelectors) + HasKeyword(keywords, Config.hKeywords);
            int portrait = HasSelector(ref selectors, Config.vSelectors) + HasKeyword(keywords, Config.vKeywords);
            bool hd = HasSelector(ref selectors, Config.hdSelectors) != 0 || HasKeyword(keywords, Config.hdKeywords) != 0;
            if (widescreen > portrait) { // Horizontal
                result.Width = hd ? Config.imageSizeHDHW : Config.imageSizeHW;
                result.Height = hd ? Config.imageSizeHDHH : Config.imageSizeHH;
            } else if (portrait > widescreen) { // Vertical
                result.Width = hd ? Config.imageSizeHDVW : Config.imageSizeVW;
                result.Height = hd ? Config.imageSizeHDVH : Config.imageSizeVH;
            } else { // Regular
                result.Width = hd ? Config.imageSizeHDW : Config.imageSizeW;
                result.Height = hd ? Config.imageSizeHDH : Config.imageSizeH;
            }
            result.Prompt = string.Join(", ", selectors);
            return result;
        }

        /// <summary>
        /// Check if a keyword for some action is present in the prompt. Will not remove the keyword from the prompt.
        /// </summary>
        /// <returns>Number of matching keywords.</returns>
        static int HasKeyword(string[] promptKeywords, string[] searchedKeywords) {
            int count = 0;
            for (int i = 0; i < promptKeywords.Length; i++) {
                if (Array.BinarySearch(searchedKeywords, promptKeywords[i]) >= 0) {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Check if a selector is defined and remove it from the prompt.
        /// </summary>
        /// <returns>Number of matching selectors.</returns>
        static int HasSelector(ref string[] promptSelectors, string[] searchedSelectors) {
            int count = 0;
            for (int i = 0; i < promptSelectors.Length; i++) {
                if (Array.BinarySearch(searchedSelectors, promptSelectors[i]) >= 0) {
                    for (int j = i + 1; j < promptSelectors.Length; j++) {
                        promptSelectors[j - 1] = promptSelectors[j];
                    }
                    Array.Resize(ref promptSelectors, promptSelectors.Length - 1);
                    i--;
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Characters that separate keywords.
        /// </summary>
        static readonly char[] keywordSplits = [' ', ',', '.', '!', ';', '/'];

        /// <summary>
        /// Characters that separate selectors.
        /// </summary>
        static readonly char[] selectorSplits = [',', '.', '!', ';', '/'];
    }
}