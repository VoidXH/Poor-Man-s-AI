namespace VoidX.WPF;

/// <summary>
/// INI file handling functions.
/// </summary>
public static class IniFile {
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
    /// Parse an INI file into a dictionary.
    /// </summary>
    public static Dictionary<string, string> ParseAll(string path) {
        Dictionary<string, string> result = [];
        IniFileParser parser = new(path);
        IniFileBlock lastBlock;
        while ((lastBlock = parser.ReadNextBlock()) != null) {
            foreach (KeyValuePair<string, string> kvp in lastBlock.Values) {
                result[kvp.Key] = kvp.Value;
            }
        }
        return result;
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
    readonly string[] file = File.ReadAllLines(path);

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
