namespace PoorMansAI.Data;

/// <summary>
/// Helpers for handling Markdown content.
/// </summary>
public static class Markdown {
    /// <summary>
    /// Convert a file <paramref name="extension"/> to a Markdown code block's name of that specific language for syntax highlighting.
    /// </summary>
    public static string GetLanguageCodeword(string extension) => extension switch {
        ".cs" => "csharp",
        ".py" => "python",
        ".js" => "javascript",
        ".ts" => "typescript",
        ".html" => "html",
        ".css" => "css",
        ".json" => "json",
        ".xml" => "xml",
        ".yml" or ".yaml" => "yaml",
        ".md" => "markdown",
        ".sql" => "sql",
        ".java" => "java",
        ".cpp" or ".cc" or ".cxx" or ".c++" => "cpp",
        ".h" or ".hpp" or ".hxx" => "cpp",
        ".c" => "c",
        ".rb" => "ruby",
        ".go" => "go",
        ".rs" => "rust",
        ".sh" or ".bash" => "bash",
        ".bat" or ".cmd" => "batch",
        ".ps1" => "powershell",
        ".php" => "php",
        ".swift" => "swift",
        ".kt" or ".kts" => "kotlin",
        ".scala" => "scala",
        ".r" or ".R" => "r",
        ".lua" => "lua",
        ".pl" or ".pm" => "perl",
        ".ex" or ".exs" => "elixir",
        ".erl" or ".hrl" => "erlang",
        ".clj" or ".cljs" or ".cljc" => "clojure",
        ".dart" => "dart",
        ".vue" => "vue",
        ".svelte" => "svelte",
        ".graphql" or ".gql" => "graphql",
        ".toml" => "toml",
        ".ini" or ".cfg" or ".conf" => "ini",
        ".dockerfile" or "dockerfile" => "dockerfile",
        ".makefile" or "makefile" or ".mk" => "makefile",
        ".proto" => "protobuf",
        ".tf" or ".hcl" => "hcl",
        ".v" => "verilog",
        ".vhd" or ".vhdl" => "vhdl",
        ".asm" or ".s" or ".S" => "assembly",
        ".zig" => "zig",
        ".nim" => "nim",
        ".cr" => "crystal",
        ".jl" => "julia",
        ".m" or ".mm" => "matlab",
        ".mjs" => "javascript",
        ".cjs" => "javascript",
        ".tsx" => "tsx",
        ".jsx" => "jsx",
        _ => string.Empty
    };
}
