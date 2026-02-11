using System.Diagnostics;
using VoidX.WPF;

namespace PoorMansAI.Engines;

partial class StableDiffusionWebUI {
    /// <summary>
    /// Setup the batch environment for the <paramref name="root"/> folder of Stable Diffusion WebUI.
    /// </summary>
    static void PrepareForWindows(string root) {
        string dir = Path.Combine(root, "system");
        Environment.SetEnvironmentVariable("PATH", string.Join(';', Path.Combine(dir, "git", "bin"), Path.Combine(dir, "python"),
                Path.Combine(dir, "python", "Scripts"), Environment.GetEnvironmentVariable("PATH")));
        Environment.SetEnvironmentVariable("PY_LIBS", Path.Combine(dir, "python", "Scripts", "Lib") + ';' +
            Path.Combine(dir, "python", "Scripts", "Lib", "site-packages"));
        Environment.SetEnvironmentVariable("PY_PIP", Path.Combine(dir, "python", "Scripts"));
        Environment.SetEnvironmentVariable("SKIP_VENV", "1");
        Environment.SetEnvironmentVariable("PIP_INSTALLER_LOCATION", Path.Combine(dir, "python", "get-pip.py"));
        Environment.SetEnvironmentVariable("TRANSFORMERS_CACHE", Path.Combine(dir, "transformers-cache"));
        Environment.SetEnvironmentVariable("PYTHON", string.Empty);
        Environment.SetEnvironmentVariable("GIT", string.Empty);
        Environment.SetEnvironmentVariable("VENV_DIR", string.Empty);
        Environment.SetEnvironmentVariable("SD_WEBUI_LOG_LEVEL", "WARNING"); // Performance + we handle it
    }

    /// <summary>
    /// Prepare the environment for Mac, because after WebUI was downloaded, it's not ready for use.
    /// </summary>
    static void PrepareForMac(string dir) {
        string path = Path.Combine(dir, "webui.sh");

        // Fix line endings
        IEnumerable<string> shFiles = Directory.EnumerateFiles(dir, "*.sh", SearchOption.AllDirectories);
        foreach (string filename in shFiles) {
            FixLineEndings(filename);
        }

        // Make it executable
        Process chmod = Process.Start(new ProcessStartInfo() {
            FileName = "/bin/bash",
            Arguments = $"-c \"chmod +x '{path}'\"",
            UseShellExecute = false
        });
        chmod.WaitForExit();
    }

    /// <summary>
    /// Use LF instead of CRLF on Mac.
    /// </summary>
    static void FixLineEndings(string path) {
        string text = File.ReadAllText(path);
        if (text.Contains("\r\n")) {
            text = text.Replace("\r", string.Empty);
            File.WriteAllText(path, text);
            Logger.Info($"Fixed line endings of {Path.GetFileName(path)}.");
        }
    }
}
