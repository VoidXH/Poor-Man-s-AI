using System.Diagnostics;
using System.Runtime.InteropServices;

using VoidX.WPF;

/// <summary>
/// Handles a single shell session. Takes an input from <see cref="SendInput(string)"/>, and constantly outputs the results to <see cref="OnOutputReceived"/>.
/// </summary>
public class ShellSession : IDisposable {
    /// <summary>
    /// Handle for the running shell instance.
    /// </summary>
    readonly Process shellProcess;

    /// <summary>
    /// Provides commands to the <see cref="shellProcess"/>.
    /// </summary>
    readonly StreamWriter inputWriter;

    /// <summary>
    /// The <see cref="shellProcess"/> was manually stopped.
    /// </summary>
    bool disposed;

    /// <summary>
    /// Calls back periodically with strings written to the shell.
    /// </summary>
    public Action<string> OnOutputReceived { get; set; }

    /// <summary>
    /// Handles a single shell session. Takes an input from <see cref="SendInput(string)"/>, and constantly outputs the results to <see cref="OnOutputReceived"/>.
    /// </summary>
    public ShellSession() {
        string fileName;
        string arguments = string.Empty;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            fileName = "cmd.exe";
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            fileName = "/bin/bash";
            arguments = "-i";
        } else {
            throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        ProcessStartInfo startInfo = new() {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        shellProcess = new Process {
            StartInfo = startInfo
        };

        shellProcess.OutputDataReceived += (sender, e) => HandleOutput(e.Data);
        shellProcess.ErrorDataReceived += (sender, e) => HandleOutput(e.Data);
        shellProcess.Start();
        shellProcess.BeginOutputReadLine();
        shellProcess.BeginErrorReadLine();
        inputWriter = shellProcess.StandardInput;
    }

    /// <summary>
    /// Sends a command to the active shell.
    /// </summary>
    public void SendInput(string command) {
        if (disposed || shellProcess.HasExited) {
            throw new ObjectDisposedException(nameof(ShellSession), "The shell process has already exited or been disposed.");
        }

        inputWriter.WriteLine(command);
    }

    /// <summary>
    /// Redirect the shell's output to the subscribers.
    /// </summary>
    void HandleOutput(string data) {
        if (data != null) {
            OnOutputReceived?.Invoke(data);
        }
    }

    /// <inheritdoc/>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Stop the shell process when we're done with it.
    /// </summary>
    protected virtual void Dispose(bool disposing) {
        if (disposed) {
            return;
        }

        if (disposing) {
            inputWriter?.Dispose();
        }

        if (shellProcess != null && !shellProcess.HasExited) {
            try {
                shellProcess.Kill();
            } catch (Exception e) {
                Logger.Error("Error while disposing shell session: " + e.Message);
            } finally {
                shellProcess.Dispose();
            }
        }

        disposed = true;
    }

    /// <summary>
    /// Safety net when the object was not disposed properly.
    /// </summary>
    ~ShellSession() => Dispose(false);
}
