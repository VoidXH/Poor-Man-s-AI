namespace PoorMansAI.Configuration {
    /// <summary>
    /// User-specific configuration values.
    /// </summary>
    public static partial class Config {
        /// <summary>
        /// Where to poll the commands from.
        /// </summary>
        public static readonly string publicWebserver = Values["PublicWebserver"];

        /// <summary>
        /// Send server messages at least this many milliseconds apart to prevent Cloudflare throttling.
        /// </summary>
        public static readonly int serverPollInterval = int.Parse(Values["ServerPollInterval"]);

        /// <summary>
        /// Priority of the chat engine across all distributed nodes.
        /// </summary>
        public static readonly int chatWeight = int.Parse(Values["ChatWeight"]);

        /// <summary>
        /// Priority of the chat engine across all distributed nodes.
        /// </summary>
        public static readonly int imageGenWeight = int.Parse(Values["ImageGenWeight"]);

        /// <summary>
        /// Command poller user's name.
        /// </summary>
        internal static readonly string adminUsername = Values["AdminUsername"];

        /// <summary>
        /// Command poller user's password.
        /// </summary>
        internal static readonly string adminPassword = Values["AdminPassword"];

        /// <summary>
        /// CPU and GPU use the same memory.
        /// </summary>
        internal static readonly bool unified = bool.Parse(Values["Unified"]);

        /// <summary>
        /// Class names of enabled extensions.
        /// </summary>
        internal static string[] extensions = Values["Extensions"].Split(',');

        /// <summary>
        /// The config file in the application folder with the highest weight.
        /// </summary>
        internal static Dictionary<string, string> Values {
            get {
                if (loadedConfig != null) {
                    return loadedConfig;
                }
                string[] configs = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"), "*.ini");
                Dictionary<string, string> result = null;
                int maxWeight = int.MinValue;
                foreach (var iniFile in configs) {
                    Dictionary<string, string> data = ParseINI(iniFile);
                    if (data.TryGetValue("ChatWeight", out string chatWeightString) && data.TryGetValue("ImageGenWeight", out string imageGenWeightString) &&
                        int.TryParse(chatWeightString, out int chatWeight) && int.TryParse(imageGenWeightString, out int imageGenWeight)) {
                        int bigger = Math.Max(chatWeight, imageGenWeight);
                        if (bigger > maxWeight) {
                            maxWeight = bigger;
                            result = data;
                        }
                    }
                }
                return loadedConfig = result;
            }
        }
        static Dictionary<string, string> loadedConfig;

        /// <summary>
        /// Parse a <paramref name="key"/> that contains a comma-separated list and trim whitespaces of the entries.
        /// </summary>
        internal static string[] ReadList(string key) {
            string source = Values[key];
            if (source.Length == 0) {
                return [];
            }
            string[] result = source.Split(',');
            for (int i = 0; i < result.Length; i++) {
                result[i] = result[i].Trim().ToLowerInvariant();
            }
            return result;
        }

        /// <summary>
        /// Enumerates a user-set collection and returns the prefix for each of them.
        /// </summary
        static IEnumerable<string> ForEach(string prefix) {
            int checkedId = 1;
            string checkedModel;
            while (true) {
                checkedModel = prefix + checkedId;
                if (!Values.ContainsKey(checkedModel)) {
                    break;
                }
                yield return checkedModel;
                checkedId++;
            }
        }

        /// <summary>
        /// Parse a configuration file.
        /// </summary>
        static Dictionary<string, string> ParseINI(string path) {
            Dictionary<string, string> result = [];
            foreach (string line in File.ReadLines(path)) {
                if (line.Length == 0 || line.StartsWith('[') || line.StartsWith(';')) {
                    continue;
                }

                int idx = line.IndexOf('=');
                if (idx != -1) {
                    string key = line[..idx];
                    string value = line[(idx + 1)..];
                    if (key.Length > 0) {
                        result[key] = value;
                    }
                }
            }
            return result;
        }
    }
}