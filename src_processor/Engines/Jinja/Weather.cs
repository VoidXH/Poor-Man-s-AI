using System.Text.Json.Nodes;

using PoorMansAI.Configuration;
using PoorMansAI.Engines.BaseClasses;
using PoorMansAI.Engines.Jinja.BaseClasses;

namespace PoorMansAI.Engines.Jinja;

/// <summary>
/// Fetches weather information for a given location.
/// </summary>
public sealed class Weather : Tool {
    /// <inheritdoc/>
    public override string Execute(LlamaCpp caller, JsonNode parameters, Command command, Engine.Progress progressReporter) {
        string location = parameters["location"]?.ToString();
        if (location == null) {
            return Config.internalErrorMessage;
        }
        char unit = parameters["unit"]?.ToString() == "fahrenheit" ? 'u' : 'm';
        string lang = parameters["language"]?.ToString() ?? "en";
        return $"<img src=\"https://wttr.in/{location}_0.png?{unit}&lang={lang}\"/>";
    }
}
