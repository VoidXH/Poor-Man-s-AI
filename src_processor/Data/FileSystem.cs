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

        if (Directory.GetDirectories(folder).Length == 0 && Directory.GetFiles(folder).Length == 0) {
            return "<p>The folder is empty.</p>";
        }

        StringBuilder result = new();
        Array.Sort(skipped);
        BuildTree(result, new(folder), folder, folder, skipped);
        return result.ToString();
    }

    /// <summary>
    /// Recursively build a HTML unordered list tree of all folders and files.
    /// </summary>
    /// <param name="currentDir">The directory whose direct children are being enumerated in this call.</param>
    /// <param name="repoRoot">The original root the tree started from; used as the gitignore base path so
    /// repo-root-relative patterns and negations (e.g. <c>!Assets/**</c>) keep matching during recursion.</param>
    static void BuildTree(StringBuilder result, GitIgnoreParser gitIgnoreParser, string currentDir, string repoRoot, string[] skipped) {
        try {
            string[] folders = Directory.GetDirectories(currentDir);
            string[] files = Directory.GetFiles(currentDir);
            result.Append("<ul>");

            foreach (string folder in folders.OrderBy(d => Path.GetFileName(d))) {
                string name = Path.GetFileName(folder);
                if (Array.BinarySearch(skipped, name) >= 0) {
                    continue;
                }
                if (gitIgnoreParser.IsIgnored(folder, repoRoot)) {
                    continue;
                }

                result.Append($"<li><b>{name}</b>");
                BuildTree(result, gitIgnoreParser, folder, repoRoot, skipped);
                result.Append("</li>");
            }

            foreach (string file in files.OrderBy(f => Path.GetFileName(f))) {
                string name = Path.GetFileName(file);
                if (gitIgnoreParser.IsIgnored(file, repoRoot)) {
                    continue;
                }

                string path = file.Replace("\\", "\\\\");
                result.Append($"<li><a href=\"javascript:void(0)\" onclick=\"sendCommandByPrompt('File:{path}')\">{name}</a> <button class=\"btn btn-secondary btn-sm p-0\" onclick=\"prependFileCommand('{path}')\">+</button></li>");
            }

            result.Append("</ul>");
        } catch (Exception ex) {
            result.Append($"<li><span style=\"color:red;\">Error: {ex.Message}</span></li>");
        }
    }
}
