using PoorMansAI.Data;

namespace PoorMansAI.Engines;

partial class AgentEngine {
    /// <summary>
    /// Handler for extra commands enclosed in square brackets at the start of a prompt.
    /// If the command is valid, it's removed from the prompt and its result is appended to the result.
    /// </summary>
    static string ExtraCommandHandler(string projectFolder, string command) => command switch {
        "Files" => FileSystem.GetTree(projectFolder, ".git", ".vs", "bin", "obj", "Library"),
        "GitDiff" => ChangedFiles.GetAllDiffs(projectFolder),
        "GitStatus" => string.Join("<br>", ChangedFiles.GetChangedFiles(projectFolder)),
        _ => null,
    };
}
