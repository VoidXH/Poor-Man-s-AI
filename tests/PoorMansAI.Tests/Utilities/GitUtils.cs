using System.Diagnostics;

using PoorMansAI.Engines.Utilities;

namespace PoorMansAI.Tests.Utilities;

/// <summary>
/// Perform operations with git.
/// </summary>
static class GitUtils {
    /// <summary>
    /// Perform an operation with git.
    /// </summary>
    public static void RunGit(string workingDir, string arguments) {
        ProcessStartInfo info = ProcessUtils.CreateRedirectedStartInfo("git", workingDir);
        info.Arguments = arguments;

        Process process = new() {
            StartInfo = info
        };
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0) {
            throw new Exception($"Git command failed: git {arguments}\nStdOut: {output}\nStdErr: {error}");
        }
    }
}
