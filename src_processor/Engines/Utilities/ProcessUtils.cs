using System.Diagnostics;
using System.Text;

namespace PoorMansAI.Engines.Utilities;

/// <summary>
/// Extension functions for <see cref="Process"/>es.
/// </summary>
public static class ProcessUtils {
    /// <summary>
    /// Stop a <paramref name="process"/> forcefully.
    /// </summary>
    public static void KillSafe(this Process process) {
        try {
            if (!process.HasExited) {
                process.Kill(true);
            }
        } catch {
            // Prevent exception if process died just as we tried to kill it
        }
    }

    /// <summary>
    /// Create a new process of which the standard output and error are redirected and could be read from the <see cref="Process"/>.
    /// </summary>
    public static ProcessStartInfo CreateRedirectedStartInfo(string path, string workingDir) => new() {
        FileName = path,
        WorkingDirectory = workingDir,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8,
        UseShellExecute = false,
        CreateNoWindow = true
    };
}
