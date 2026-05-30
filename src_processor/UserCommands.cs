using System.Globalization;
using System.Net;

using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Engines;

namespace PoorMansAI;

/// <summary>
/// Submits, checks, and stops commands via the website's user-facing endpoints.
/// </summary>
public class UserCommands {
    /// <summary>
    /// Cookies received when logged in successfully.
    /// </summary>
    CookieCollection cookies;

    /// <summary>
    /// Submits, checks, and stops commands via the website's user-facing endpoints.
    /// </summary>
    public UserCommands() {
        string loginUrl = HTTP.Combine(Config.publicWebserver, "/login.php");
        cookies = HTTP.GetCookies(loginUrl, [
            new KeyValuePair<string, string>("name", Config.adminUsername),
            new KeyValuePair<string, string>("password", Config.adminPassword)
        ]);
    }

    /// <summary>
    /// Submit a command to the server and return the assigned command ID.
    /// </summary>
    /// <param name="engineType">The engine type to run the command on.</param>
    /// <param name="prompt">The user's prompt text.</param>
    /// <returns>The command ID on success, or -1 on failure.</returns>
    public int SubmitCommand(EngineType engineType, string prompt) {
        string command = $"{engineType}|{prompt}";
        string result = HTTP.POST(
            HTTP.Combine(Config.publicWebserver, "/commands.php"),
            [new("command", command)],
            cookies
        );

        if (result == null) {
            Logger.Debug("Failed to submit command: HTTP error.");
            return -1;
        }

        if (!int.TryParse(result.Trim(), out int id)) {
            Logger.Debug($"Failed to parse command ID from server response: '{result}'");
            return -1;
        }

        Logger.Debug($"Command submitted with ID {id}.");
        return id;
    }

    /// <summary>
    /// Check the status of a command.
    /// </summary>
    /// <param name="commandId">The ID of the command to check.</param>
    /// <returns>A tuple of (success, progress, status). The <paramref name="success"/> value is false on error.</returns>
    public (bool success, int progress, string status) CheckCommand(int commandId) {
        string result = HTTP.GET(
            HTTP.Combine(Config.publicWebserver, "/commands.php") + "?check=" + commandId,
            cookies
        );

        if (result == null) {
            Logger.Debug($"Failed to check command {commandId}: HTTP error.");
            return (false, 0, string.Empty);
        }

        int pipeIndex = result.LastIndexOf('|');

        if (pipeIndex == -1) {
            Logger.Debug($"Malformed response from server for command {commandId}: '{result}'");
            return (false, 0, string.Empty);
        }

        string progressStr = result[..pipeIndex];
        string status = result[(pipeIndex + 1)..];

        if (!int.TryParse(progressStr, CultureInfo.InvariantCulture, out int progress)) {
            Logger.Debug($"Failed to parse progress from server response for command {commandId}: '{progressStr}'");
            return (false, 0, string.Empty);
        }

        return (true, progress, status);
    }

    /// <summary>
    /// Stop (cancel) a running command.
    /// </summary>
    /// <param name="commandId">The ID of the command to stop.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool StopCommand(int commandId) {
        string result = HTTP.POST(
            HTTP.Combine(Config.publicWebserver, "/commands.php"),
            [new("stop", commandId.ToString(CultureInfo.InvariantCulture))],
            cookies
        );

        bool success = result != null;
        if (!success) {
            Logger.Debug($"Failed to stop command {commandId}: HTTP error.");
        }
        return success;
    }
}
