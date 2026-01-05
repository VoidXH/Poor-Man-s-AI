using System.Diagnostics;

using VoidX.WPF;

namespace PoorMansAI.Engines;

partial class StableDiffusionWebUI {
    /// <summary>
    /// Selectively print llama.cpp logs based on the current log level.
    /// </summary>
    void SanitizeLog(object _, DataReceivedEventArgs e) {
        if (e.Data == null) {
            return;
        }

        string line = e.Data;
        if (Logger.MinLogLevel > LogLevel.Debug) {
            for (int i = 0; i < skippedLineStarts.Length; i++) {
                if (line.StartsWith(skippedLineStarts[i])) {
                    return;
                }
            }

            if ((line.StartsWith("INFO:") && !line.Contains("http")) || line.EndsWith("200 OK")) {
                return;
            }

        }

        if (line.Length > 4 && line[3] == '%' && line[4] == '|') { // Generation progress bar
            Console.Write('\r');
            Console.Write(line);
        } else if (!string.IsNullOrWhiteSpace(line)) {
            Logger.Log("SD WebUI", line, ConsoleColor.DarkMagenta, false);
        }
    }

    /// <summary>
    /// Recommended command line arguments when Stable Diffusion WebUI is ran on a Mac.
    /// </summary>
    const string macArgs = "--skip-torch-cuda-test --upcast-sampling --use-cpu interrogate";

    /// <summary>
    /// Lines starting with these are only logged when <see cref="Logger.MinLogLevel"/> is lower or equal than <see cref="LogLevel.Debug"/>.
    /// </summary>
    static readonly string[] skippedLineStarts = [
        "#####",
        "  warnings.warn",
        "Applying attention",
        "Commit hash:",
        "Install script",
        "No module", "no module",
        "Python ",
        "Reusing loaded model",
        "Tested on",
        "Version:",
        "WARNING:root:Sampler Scheduler",
    ];
}
