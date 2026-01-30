using VoidX.WPF;

namespace PoorMansAI.Configuration;

/// <summary>
/// User-specific configuration values.
/// </summary>
public static partial class Config {
    /// <summary>
    /// Message displayed on internal server errors. The errors will be logged to the console.
    /// </summary>
    public static readonly string internalErrorMessage = "An internal error has occured, please try again.";

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
    internal static string[] extensions = Values["Extensions"].Split(',', StringSplitOptions.TrimEntries);

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
                Dictionary<string, string> data = IniFile.ParseAll(iniFile);
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
    /// Parse keywords or selectors to a binary searchable (sorted) array.
    /// </summary>
    internal static string[] GetKeywordList(string value) => GetKeywordList(GetList(value));

    /// <summary>
    /// Parse keywords or selectors to a binary searchable (sorted) array.
    /// </summary>
    internal static string[] ReadKeywordList(string key) => GetKeywordList(ReadList(key));

    /// <summary>
    /// Parse a <paramref name="key"/> that contains a comma-separated list and trim whitespaces of the entries.
    /// </summary>
    internal static string[] ReadList(string key) => GetList(Values[key]);

    /// <summary>
    /// Parse keywords or selectors to a binary searchable (sorted) array.
    /// </summary>
    internal static string[] GetKeywordList(string[] source) {
        for (int i = 0; i < source.Length; i++) {
            source[i] = source[i].ToLowerInvariant();
        }
        Array.Sort(source);
        return source;
    }

    /// <summary>
    /// Parse a <paramref name="source"/> comma-separated list and trim whitespaces of the entries.
    /// </summary>
    static string[] GetList(string source) {
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
}
