using System.Runtime.CompilerServices;

namespace VoidX.WPF;

/// <summary>
/// Possible logging categories.
/// </summary>
public enum LogLevel {
    /// <summary>
    /// Message intended for developers.
    /// </summary>
    Debug,

    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Submodule fault, operation can continue.
    /// </summary>
    Warning,

    /// <summary>
    /// Application fault, operation can't be continued.
    /// </summary>
    Error
}

/// <summary>
/// Outputs log messages to the corresponding console.
/// </summary>
public static class Logger {
    /// <summary>
    /// The lowest level to log.
    /// </summary>
    public static LogLevel MinLogLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Handles thread safety.
    /// </summary>
    readonly static object locker = new();

    /// <summary>
    /// Outputs log messages to the corresponding console.
    /// </summary>
    public static void Log(LogLevel level, string message) {
        switch (level) {
            case LogLevel.Debug:
                Log("DEBUG", message, ConsoleColor.DarkCyan, false);
                break;
            case LogLevel.Info:
                Log("INFO", message, ConsoleColor.Green, false);
                break;
            case LogLevel.Warning:
                Log("WARN", message, ConsoleColor.Yellow, false);
                break;
            case LogLevel.Error:
                Log("ERROR", message, ConsoleColor.Red, true);
                break;
        }
    }

    /// <summary>
    /// Log a debug message to the standard output.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug(string message) {
        if (LogLevel.Debug >= MinLogLevel) {
            Log(LogLevel.Debug, message);
        }
    }

    /// <summary>
    /// Log a debug message to the standard output with <paramref name="args"/> to insert to the <paramref name="message"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug(string message, params object[] args) {
        if (LogLevel.Debug >= MinLogLevel) {
            Log(LogLevel.Debug, string.Format(message, args));
        }
    }

    /// <summary>
    /// Log an info message to the standard output.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(string message) {
        if (LogLevel.Info >= MinLogLevel) {
            Log(LogLevel.Info, message);
        }
    }

    /// <summary>
    /// Log a warning message to the standard output.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warning(string message) {
        if (LogLevel.Warning >= MinLogLevel) {
            Log(LogLevel.Warning, message);
        }
    }

    /// <summary>
    /// Log a warning message to the error output.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(string message) {
        if (LogLevel.Error >= MinLogLevel) {
            Log(LogLevel.Error, message);
        }
    }

    /// <summary>
    /// Assemble a log line with per-level properties.
    /// </summary>
    public static void Log(string level, string message, ConsoleColor color, bool error) {
        lock (locker) {
            TextWriter console = error ? Console.Error : Console.Out;
            console.Write('[');
            Console.ForegroundColor = color;
            console.Write(level);
            Console.ResetColor();
            console.Write("] ");
            console.WriteLine(message);
        }
    }
}
