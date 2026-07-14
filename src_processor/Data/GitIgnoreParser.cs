using System.Text.RegularExpressions;

namespace PoorMansAI.Data;

/// <summary>
/// Parses and matches .gitignore patterns.
/// </summary>
public class GitIgnoreParser {
    record IgnorePattern(string Pattern, Regex Regex, bool Negated, bool DirectoryOnly);

    readonly List<IgnorePattern> patterns = [];

    public GitIgnoreParser(string folder) {
        bool isRepoRoot = Directory.Exists(Path.Combine(folder, ".git")) || File.Exists(Path.Combine(folder, ".git"));
        if (!isRepoRoot) {
            string parent = Directory.GetParent(folder)?.FullName;
            if (parent != null && parent != folder) {
                string parentGitignore = Path.Combine(parent, ".gitignore");
                if (File.Exists(parentGitignore)) {
                    ParseFile(parentGitignore);
                }
            }
        }

        string gitignorePath = Path.Combine(folder, ".gitignore");
        if (File.Exists(gitignorePath)) {
            ParseFile(gitignorePath);
        }
    }

    static string PatternToRegex(string pattern) {
        bool anchored = pattern.StartsWith('/');
        if (anchored) {
            pattern = pattern[1..];
        }

        bool matchAnywhere = !pattern.Contains('/');

        string regexPattern = Regex.Escape(pattern)
            .Replace(@"\*\*/", ".*")      // Handle **/
            .Replace(@"\*\*", ".*")       // Handle ** (must be before \* replacement)
            .Replace(@"\*", "[^/]*")      // Handle *
            .Replace(@"\?", "[^/]");      // Handle ?

        string prefix = anchored ? "^" : (matchAnywhere ? @"(^|/)" : "^");
        string suffix = @"($|/)";
        return prefix + regexPattern + suffix;
    }

    public bool IsIgnored(string path, string basePath) {
        if (patterns.Count == 0) {
            return false;
        }

        string relative = path.StartsWith(basePath) ?
            path[basePath.Length..].Replace('\\', '/') :
            path.Replace('\\', '/');

        relative = relative.Trim('/');

        bool ignored = false;
        for (int i = patterns.Count - 1; i >= 0; i--) {
            IgnorePattern p = patterns[i];

            if (p.DirectoryOnly && !Directory.Exists(path)) {
                continue;
            }

            if (p.Regex.IsMatch(relative)) {
                ignored = !p.Negated;
                break; // Found the decisive rule
            }
        }
        return ignored;
    }

    void ParseFile(string path) {
        string[] lines = File.ReadAllLines(path);
        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#')) {
                continue;
            }

            bool negated = line.StartsWith('!');
            string pattern = negated ? line[1..] : line;

            bool directoryOnly = pattern.EndsWith('/');
            if (directoryOnly) {
                pattern = pattern.TrimEnd('/');
            }

            string regexStr = PatternToRegex(pattern);
            try {
                Regex compiled = new(regexStr, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                patterns.Add(new(line, compiled, negated, directoryOnly));
            } catch {
                // Skip invalid patterns
            }
        }
    }
}
