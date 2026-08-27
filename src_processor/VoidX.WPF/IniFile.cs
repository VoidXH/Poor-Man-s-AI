using System.Text.RegularExpressions;

namespace VoidX.WPF;

/// <summary>
/// INI file handling functions.
/// </summary>
public static partial class IniFile {
    /// <summary>
    /// Regex matching %variable% patterns for batch variable substitution.
    /// </summary>
    static readonly Regex VariableRegex = CreateVariableRegex();
    [GeneratedRegex(@"%([^%]+)%", RegexOptions.Compiled)]
    private static partial Regex CreateVariableRegex();

    /// <summary>
    /// Variables loaded once from Variables.ini in the Configuration directory.
    /// </summary>
    static readonly Dictionary<string, string> Variables = LoadVariables();

    /// <summary>
    /// Parse all blocks in an INI file.
    /// </summary>
    public static IniFileBlock[] Parse(string path) {
        List<IniFileBlock> blocks = [];
        IniFileParser parser = new(path);
        IniFileBlock lastBlock;
        while ((lastBlock = parser.ReadNextBlock()) != null) {
            blocks.Add(lastBlock);
        }
        return [.. blocks];
    }

    /// <summary>
    /// Parse an INI file into a dictionary, resolving %variable% references from Variables.ini.
    /// </summary>
    public static Dictionary<string, string> ParseAll(string path) {
        Dictionary<string, string> result = [];
        IniFileParser parser = new(path);
        IniFileBlock lastBlock;
        while ((lastBlock = parser.ReadNextBlock()) != null) {
            foreach (KeyValuePair<string, string> kvp in lastBlock.Values) {
                result[kvp.Key] = ResolveVariables(kvp.Value);
            }
        }
        return result;
    }

    /// <summary>
    /// Load variables from Variables.ini located in the Configuration directory.
    /// </summary>
    static Dictionary<string, string> LoadVariables() {
        string configDir = Path.Combine(AppContext.BaseDirectory, "Configuration");
        string variablesPath = Path.Combine(configDir, "Variables.ini");
        return File.Exists(variablesPath) ? ParseAll(variablesPath) : [];
    }

    /// <summary>
    /// Replace all %variable% patterns in <paramref name="value"/> with values from Variables.ini. Unresolved patterns are left as-is.
    /// </summary>
    static string ResolveVariables(string value) {
        return VariableRegex.Replace(value, match => {
            string key = match.Groups[1].Value;
            return Variables.TryGetValue(key, out string replacement) ? replacement : match.Value;
        });
    }
}

/// <summary>
/// Represents an ini file's block that's separated with a header.
/// </summary>
/// <param name="Header">Name of the block</param>
/// <param name="Values">All key-value pairs in the block, indexable by key</param>
public record class IniFileBlock(string Header, Dictionary<string, string> Values) {
    /// <summary>
    /// Get a value by its key.
    /// </summary>
    public string this[string key] => Values[key];
}

/// <summary>
/// Helps parsing INI files.
/// </summary>
class IniFileParser(string path) {
    /// <summary>
    /// The entire ini file read into lines.
    /// </summary>
    readonly string[] file = ReadLines(path);

    static string[] ReadLines(string path, HashSet<string> visited = null) {
        visited ??= new(StringComparer.OrdinalIgnoreCase);
        string fullPath = Path.GetFullPath(path);
        if (!visited.Add(fullPath) || !File.Exists(fullPath)) {
            return [];
        }

        List<string> result = [];
        string dir = Path.GetDirectoryName(fullPath) ?? string.Empty;
        foreach (string rawLine in File.ReadAllLines(fullPath)) {
            string trimmed = rawLine.Trim();
            if (trimmed.StartsWith("Import(", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(')')) {
                string importFileName = trimmed[7..^1].Trim();
                string importPath = Path.IsPathRooted(importFileName) ? importFileName : Path.Combine(dir, importFileName);
                result.AddRange(ReadLines(importPath, visited));
            } else {
                result.Add(rawLine);
            }
        }
        return [.. result];
    }

    /// <summary>
    /// Number of lines read so far.
    /// </summary>
    int currentLine;

    /// <summary>
    /// Next header found in the file.
    /// </summary>
    string nextHeader = string.Empty;

    /// <summary>
    /// Parse the values under the next header.
    /// </summary>
    public IniFileBlock ReadNextBlock() {
        if (currentLine == file.Length) {
            return null;
        }

        Dictionary<string, string> values = [];
        while (currentLine < file.Length) {
            string line = file[currentLine++];
            if (line.StartsWith('[') && line.EndsWith(']')) {
                IniFileBlock result = values.Count != 0 ?
                    new(nextHeader, values) :
                    null;
                nextHeader = line[1..^1];
                if (result != null) {
                    return result;
                }
            }

            if (line.Length == 0 || line.StartsWith(';')) {
                continue;
            }

            int idx = line.IndexOf('=');
            if (idx != -1) {
                string key = line[..idx];
                string value = line[(idx + 1)..];
                if (key.Length > 0) {
                    values[key] = value;
                }
            }
        }
        return new(nextHeader, values);
    }
}
