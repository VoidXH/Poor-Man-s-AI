using System.Runtime.CompilerServices;

using VoidX.WPF;

using PoorMansAI.Configuration;

namespace PoorMansAI.NewTech.ContextDocTree {
    /// <summary>
    /// Based on the keywords in a chat prompt, gets which files from <see cref="Config.contextDocTree"/> can add extra knowledge to the conversation.
    /// </summary>
    public class ContextDocFinder {
        /// <summary>
        /// Entry point of the Context Doc Tree, folders and files corresponding as the first keyword match.
        /// </summary>
        readonly ContextDocTreeNode root;

        /// <summary>
        /// The Context Doc Tree system is set up and usable, even if the <see cref="root"/> object can't be used. Fall back to IO search in that case.
        /// </summary>
        readonly bool active = false;

        /// <summary>
        /// Initialize the Context Doc Tree system according to the files the user has placed in the <see cref="Config.contextDocTree"/> folder.
        /// </summary>
        public ContextDocFinder() {
            try {
                root = new(Config.contextDocTree);
                active = root.Valid;
            } catch (IOException) {
                Logger.Info("Context Doc Tree is not set up.");
            } catch (TooManyFilesException) {
                Logger.Warning("Context Doc Tree is too large, continuing without caching.");
                active = true;
            } catch (Exception e) {
                Logger.Warning("Contect Doc Tree parsing failed for: " + e.ToString());
            }
        }

        /// <summary>
        /// Find all relevant context docs, and insert them to the prompt.
        /// </summary>
        /// <param name="prompt">Prompt to augment</param>
        /// <param name="extra">Additional keywords to augment</param>
        public string TransformPrompt(string prompt, string extra = null) {
            if (!active) {
                return prompt;
            }

            string[] keywords = prompt.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (extra != null) {
                string[] extraKeywords = extra.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                Array.Resize(ref keywords, keywords.Length + extraKeywords.Length);
                extraKeywords.CopyTo(keywords, keywords.Length - extraKeywords.Length);
            }
            for (int i = 0; i < keywords.Length; i++) {
                keywords[i] = keywords[i].ToLowerInvariant();
            }
            string context = ExtractContext(keywords);
            return context.Length != 0 ? context + "\nKnowing all these, reply to the following message: " + prompt : prompt;
        }

        /// <summary>
        /// Find the corresponding context docs for the user's keywords.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        string ExtractContext(string[] keywords) {
            if (root != null) {
                return root.GetContext(keywords);
            } else {
                Logger.Warning("IO-based Context Doc Tree is not yet implemented, context docs won't be used.");
                return string.Empty; // TODO
            }
        }

        /// <summary>
        /// Separators to cut keywords by.
        /// </summary>
        static readonly char[] separators = [' ', ',', ':', ';', '.', '!', '?', '/', '(', ')', '\'', '\"'];
    }
}