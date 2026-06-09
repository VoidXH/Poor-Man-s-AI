using PoorMansAI.Data;

namespace PoorMansAI.Engines;

partial class AgentEngine {
    /// <summary>
    /// Handler for extra commands enclosed in square brackets at the start of a prompt.
    /// If the command is valid, it's removed from the prompt and its result is appended to the result.
    /// </summary>
    static string ExtraCommandHandler(string workingDir, string command) => command switch {
        "GitDiff" => ChangedFiles.GetAllDiffs(workingDir),
        "GitStatus" => string.Join("<br>", ChangedFiles.GetChangedFiles(workingDir)),
        _ => null,
    };
}
