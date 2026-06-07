using System.Diagnostics;

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
}
