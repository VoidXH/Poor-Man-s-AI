using System.Globalization;

using PoorMansAI.Configuration;
using PoorMansAI.Engines.Jinja;

namespace PoorMansAI.Engines.Models;

/// <summary>
/// Collects all info regarding a large language model.
/// </summary>
public class LLModel {
    /// <summary>
    /// Model name on the UI.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Where the model is located on the computer.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// How the LLM should behave.
    /// </summary>
    public string SystemMessage { get; }

    /// <summary>
    /// Put this to the end of all user input.
    /// </summary>
    public string PostMessage { get; }

    /// <summary>
    /// Randomness of each next token.
    /// </summary>
    public float Temperature { get; }

    /// <summary>
    /// List of supported external tools.
    /// </summary>
    public JinjaConfig Jinja { get; }

    /// <summary>
    /// Collects all info regarding a large language model.
    /// </summary>
    /// <param name="prefix">The model is referred to as this in the config file (Model1 and so on)</param>
    /// <param name="large">The file path shall be the large version that runs when the machine only processes LLMs and nothing else</param>
    public LLModel(string prefix, bool large) {
        Dictionary<string, string> config = Config.Values;
        Name = config[prefix];
        FilePath = Path.Combine(Config.models, Path.GetFileName(config[prefix + (large ? "LLM" : "SLM")]));
        SystemMessage = config[prefix + "SystemMessage"];

        string postMessageKey = prefix + "PostMessage";
        if (config.TryGetValue(postMessageKey, out string postMessage)) {
            PostMessage = postMessage;
        }

        string temperature = config.GetValueOrDefault(prefix + "Temperature", config["ChatTemperature"]);
        Temperature = float.Parse(temperature, CultureInfo.InvariantCulture);

        if (config.TryGetValue(prefix + "Jinja", out string jinja)) {
            Jinja = new(Path.Combine(Directory.GetCurrentDirectory(), "Configuration", jinja));
        }
    }

    /// <summary>
    /// Collects all info regarding a large language model.
    /// </summary>
    /// <param name="url">Where the model was downloaded from</param>
    /// <param name="systemMessage">Initial prompt defining the model's personality</param>
    public LLModel(string url, string systemMessage) {
        FilePath = Path.Combine(Config.models, Path.GetFileName(url));
        SystemMessage = systemMessage;
    }
}
