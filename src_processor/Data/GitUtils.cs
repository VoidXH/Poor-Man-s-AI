using System.Diagnostics;

using PoorMansAI.Engines.Utilities;

namespace PoorMansAI.Data;

/// <summary>
/// Perform operations with git.
/// </summary>
public static class GitUtils {
    /// <summary>
    /// Perform an operation with git, with a default timeout.
    /// </summary>
    public static string RunGit(string workingDir, string arguments) => RunGit(workingDir, arguments, defaultTimeoutSeconds);

    /// <summary>
    /// Perform an operation with git.
    /// </summary>
    /// <param name="workingDir">The working directory for the git command</param>
    /// <param name="arguments">The arguments to pass to git</param>
    /// <param name="timeoutSeconds">Maximum seconds to wait before killing the process</param>
    public static string RunGit(string workingDir, string arguments, int timeoutSeconds) {
        if (!IsGitRepo(workingDir)) {
            return notInitializedMessage;
        }

        ProcessStartInfo info = ProcessUtils.CreateRedirectedStartInfo("git", workingDir);
        info.Arguments = arguments;

        using Process process = new() {
            StartInfo = info
        };
        process.Start();

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(timeoutSeconds * 1000)) {
            process.Kill(true);
            process.WaitForExit();
            throw new TimeoutException($"Git command timed out after {timeoutSeconds} seconds: git {arguments}");
        }

        string output = outputTask.Result;
        string error = errorTask.Result;

        if (process.ExitCode != 0) {
            throw new Exception($"Git command failed: git {arguments}\nStdOut: {output}\nStdErr: {error}");
        }
        return output;
    }

    /// <summary>
    /// Checks whether the given directory or any of its parent directories contains a <c>.git</c> folder.
    /// </summary>
    /// <param name="directory">The directory to start searching from</param>
    /// <returns><c>true</c> if a <c>.git</c> folder is found in the directory or any ancestor; otherwise, <c>false</c>.</returns>
    static bool IsGitRepo(string directory) {
        string current = Path.GetFullPath(directory);
        while (!string.IsNullOrEmpty(current)) {
            string gitPath = Path.Combine(current, ".git");
            if (Directory.Exists(gitPath)) {
                return true;
            }
            string parent = Directory.GetParent(current)?.FullName;
            if (string.IsNullOrEmpty(parent) || parent == current) {
                break;
            }
            current = parent;
        }
        return false;
    }

    /// <summary>
    /// How long to wait by default before killing git for waiting too long.
    /// </summary>
    const int defaultTimeoutSeconds = 20;

    /// <summary>
    /// Message returned when no git repository is found.
    /// </summary>
    const string notInitializedMessage = "Git is not initialized in this repo.";
}
