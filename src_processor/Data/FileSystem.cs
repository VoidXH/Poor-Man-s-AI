using System.Text;

namespace PoorMansAI.Data;

/// <summary>
/// Generates an HTML unordered list tree of files in a folder.
/// </summary>
public static class FileSystem {
    /// <summary>
    /// Returns an HTML unordered list tree of all folders and files in a <paramref name="folder"/>.
    /// </summary>
    public static string GetTree(string folder, params string[] skipped) {
        if (!Directory.Exists(folder)) {
            return "<p>Folder not found.</p>";
        }

        StringBuilder result = new();
        Array.Sort(skipped);
        BuildTree(result, folder, skipped);
        return result.ToString();
    }

    /// <summary>
    /// Recursively build a HTML unordered list tree of all folders and files.
    /// </summary>
    static void BuildTree(StringBuilder result, string root, string[] skipped) {
        try {
            string[] folders = Directory.GetDirectories(root);
            string[] files = Directory.GetFiles(root);
            result.Append("<ul>");

            foreach (string folder in folders.OrderBy(d => Path.GetFileName(d))) {
                if (Array.BinarySearch(skipped, Path.GetFileName(folder)) >= 0) {
                    continue;
                }

                string name = Path.GetFileName(folder);
                result.Append($"<li><b>[DIR] {name}</b>");
                BuildTree(result, folder, skipped);
                result.Append("</li>");
            }

            foreach (string file in files.OrderBy(f => Path.GetFileName(f))) {
                string name = Path.GetFileName(file);
                result.Append($"<li>{name}</li>");
            }

            result.Append("</ul>");
        } catch (Exception ex) {
            result.Append($"<li><span style=\"color:red;\">Error: {ex.Message}</span></li>");
        }
    }
}
