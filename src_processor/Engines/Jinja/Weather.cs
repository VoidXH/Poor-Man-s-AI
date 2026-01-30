using System.Text.Json.Nodes;

using PoorMansAI.Configuration;

namespace PoorMansAI.Engines.Jinja;

/// <summary>
/// Fetches weather information for a given location.
/// </summary>
public class Weather : Tool {
    /// <inheritdoc/>
    public override string Execute(LlamaCpp caller, JsonNode parameters, Engine.Progress progressReporter) {
        string location = parameters["location"]?.ToString();
        if (location == null) {
            return Config.internalErrorMessage;
        }
        char unit = parameters["unit"]?.ToString() == "fahrenheit" ? 'u' : 'm';
        string lang = parameters["language"]?.ToString() ?? "en";
        return $"<img src=\"https://wttr.in/{location}_0.png?{unit}&lang={lang}\"/>";
    }
}
