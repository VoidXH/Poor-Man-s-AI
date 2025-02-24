using System.Text;

using VoidX.WPF;

using PoorMansAI.Configuration;

namespace PoorMansAI.NewTech.ContextDocTree {
    /// <summary>
    /// Represents a folder in the Context Doc Tree: for the found keyword, the <see cref="entries"/> are added to the conversation as
    /// extra knowledge, and if a <see cref="Children"/> keyword is found, that subset is also added for context.
    /// </summary>
    class ContextDocTreeNode {
        /// <summary>
        /// This node contains further context docs.
        /// </summary>
        public bool Valid => Children.Count != 0 || entries.Length != 0;

        /// <summary>
        /// New search paths for additional matching keywords.
        /// </summary>
        public IReadOnlyDictionary<string, ContextDocTreeNode> Children { get; private set; }

        /// <summary>
        /// Path of the folder this node represents.
        /// </summary>
        readonly string path;

        /// <summary>
        /// Documents to use when a keyword has matched.
        /// </summary>
        string[] entries;

        /// <summary>
        /// Parse a folder intended for the Context Doc Tree system.
        /// </summary>
        public ContextDocTreeNode(string folder) {
            int filesParsed = 0;
            path = folder;
            Parse(folder, ref filesParsed);
        }

        /// <summary>
        /// Parse a folder intended for the Context Doc Tree system, while keeping track of the files cached so it won't surpass
        /// <see cref="maxCachedFiles"/>.
        /// </summary>
        ContextDocTreeNode(string folder, ref int filesSoFar) {
            path = folder;
            Parse(folder, ref filesSoFar);
            if (filesSoFar > maxCachedFiles) {
                throw new TooManyFilesException();
            }
        }

        /// <summary>
        /// Find all relevant documents in a Context Doc Tree and return them as a single string.
        /// </summary>
        public string GetContext(string[] keywords) {
            StringBuilder result = new();
            if (Children.Count != 0) {
                for (int i = 0; i < keywords.Length; i++) {
                    if (Children.ContainsKey(keywords[i])) {
                        string context = Children[keywords[i]].GetContext(keywords);
                        if (context.Length != 0) {
                            result.AppendLine(context);
                        }
                    }
                }
            }

            if (entries.Length != 0) {
                if (Config.fullContext) {
                    string[] files = Directory.GetFiles(path);
                    for (int i = 0; i < files.Length; i++) {
                        if (Array.BinarySearch(textFileFormats, Path.GetExtension(files[i])) >= 0) {
                            result.AppendLine(File.ReadAllText(files[i]));
                        }
                    }
                } else {
                    for (int i = 0; i < keywords.Length; i++) {
                        if (Array.BinarySearch(entries, keywords[i]) >= 0) {
                            string[] files = Directory.GetFiles(path, keywords[i] + '*');
                            for (int j = 0; j < files.Length; j++) {
                                result.AppendLine(File.ReadAllText(files[j]));
                            }
                        }
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Perform recursive parsing of folders.
        /// </summary>
        void Parse(string folder, ref int filesSoFar) {
            string[] subfolders = Directory.GetDirectories(folder);
            Dictionary<string, ContextDocTreeNode> children = [];
            for (int i = 0; i < subfolders.Length; i++) {
                children.Add(Path.GetFileName(subfolders[i]).ToLowerInvariant(), new ContextDocTreeNode(subfolders[i], ref filesSoFar));
            }
            Children = children;

            string[] files = Directory.GetFiles(folder);
            List<string> entries = [];
            for (int i = 0; i < files.Length; i++) {
                if (Array.BinarySearch(textFileFormats, Path.GetExtension(files[i])) >= 0) {
                    string fileName = Path.GetFileNameWithoutExtension(files[i]);
                    entries.Add(fileName.ToLowerInvariant());
                    Logger.Debug("Context doc added: {0}.", fileName);
                } else {
                    Logger.Warning(Path.GetFileName(files[i]) + " was skipped because it's not in a supported format.");
                }
            }
            this.entries = [.. entries.Order()];
        }

        /// <summary>
        /// Maximum number of context doc paths to keep in memory.
        /// </summary>
        const int maxCachedFiles = 1_000_000;

        /// <summary>
        /// Text-based file formats that can be inserted to the prompt as plaintext.
        /// </summary>
        static readonly string[] textFileFormats = [".htm", ".html", ".md", ".txt"];
    }
}