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
    public static readonly string publicWebserver = GetValue("PublicWebserver");

    /// <summary>
    /// Send server messages at least this many milliseconds apart to prevent Cloudflare throttling.
    /// </summary>
    public static readonly int serverPollInterval = int.Parse(GetValue("ServerPollInterval"));

    /// <summary>
    /// Priority of the chat engine across all distributed nodes.
    /// </summary>
    public static readonly int chatWeight = int.Parse(GetValue("ChatWeight"));

    /// <summary>
    /// Priority of the chat engine across all distributed nodes.
    /// </summary>
    public static readonly int imageGenWeight = int.Parse(GetValue("ImageGenWeight"));

    /// <summary>
    /// Priority of the agent engine across all distributed nodes.
    /// </summary>
    public static readonly int agentWeight = int.Parse(GetValue("AgentWeight"));

    /// <summary>
    /// Command poller user's name.
    /// </summary>
    internal static readonly string adminUsername = GetValue("AdminUsername");

    /// <summary>
    /// Command poller user's password.
    /// </summary>
    internal static readonly string adminPassword = GetValue("AdminPassword");

    /// <summary>
    /// CPU and GPU use the same memory.
    /// </summary>
    internal static readonly bool unified = bool.Parse(GetValue("Unified"));

    /// <summary>
    /// Class names of enabled extensions.
    /// </summary>
    internal static string[] extensions = GetValue("Extensions").Split(',', StringSplitOptions.TrimEntries);

    /// <summary>
    /// The config file in the application folder with the highest weight.
    /// </summary>
    static Dictionary<string, string> Values {
        get {
            if (loadedConfig != null) {
                return loadedConfig;
            }
            string[] configs = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"), "*.ini");
            Dictionary<string, string> result = null;
            int maxWeight = int.MinValue;
            foreach (var iniFile in configs) {
                Dictionary<string, string> data = IniFile.ParseAll(iniFile);
                if (!data.ContainsKey("ChatWeight")) {
                    continue;
                }

                int GetWeight(string key) => int.TryParse(data[key], out int weight) ? weight : int.MinValue;
                int currentMax = Math.Max(GetWeight("ChatWeight"), Math.Max(GetWeight("ImageGenWeight"), GetWeight("AgentWeight")));
                if (currentMax > maxWeight) {
                    maxWeight = currentMax;
                    result = data;
                }
            }
            return loadedConfig = result;
        }
    }
    static Dictionary<string, string> loadedConfig;

    /// <summary>
    /// Returns the configuration value associated with <paramref name="key"/>. If the key is missing, logs a
    /// clear message to the console and terminates the application before a confusing <see cref="KeyNotFoundException"/> is thrown.
    /// </summary>
    internal static string GetValue(string key) {
        Dictionary<string, string> config = Values;
        if (config.TryGetValue(key, out string value)) {
            return value;
        }
        Logger.Error($"{key} is missing from the highest priority configuration file. Please check its integrity.");
        Environment.Exit(1);
        return null;
    }

    /// <summary>
    /// Tries to read an optional configuration value. Returns <c>false</c> (without crashing) if the <paramref name="key"/> is absent.
    /// </summary>
    internal static bool TryGetValue(string key, out string value) => Values.TryGetValue(key, out value);

    /// <summary>
    /// Reads an optional configuration value, returning <paramref name="defaultValue"/> if the <paramref name="key"/> is absent.
    /// </summary>
    internal static string GetValueOrDefault(string key, string defaultValue) => TryGetValue(key, out string value) ? value : defaultValue;

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
    internal static string[] ReadList(string key) => GetList(GetValue(key));

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
    /// Enumerates the user-set agents and returns the prefix for each of them.
    /// </summary>
    internal static IEnumerable<string> ForEachAgent() => ForEach("Agent");

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
