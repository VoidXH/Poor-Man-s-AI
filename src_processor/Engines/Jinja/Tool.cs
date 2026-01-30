using System.Text.Json.Nodes;

using VoidX.WPF;

using PoorMansAI.Configuration;

namespace PoorMansAI.Engines.Jinja;

/// <summary>
/// An externally usable tool for Jinja.
/// </summary>
public abstract class Tool {
    /// <summary>
    /// Perform an operation with this tool.
    /// </summary>
    /// <param name="caller">The calling engine, and with the <see cref="Engine.Others"/> property, it can see some other engines</param>
    /// <param name="parameters">Parameters for this called tool as output by the model</param>
    /// <param name="progressReporter">Callback with progress reporting for displaying partial progress to the user</param>
    /// <returns>What is put to the chat output.</returns>
    public abstract string Execute(LlamaCpp caller, JsonNode parameters, Engine.Progress progressReporter);

    /// <summary>
    /// Parse a tool from a configuration file.
    /// </summary>
    public static (Tool engine, JsonObject jinja) Parse(IniFileBlock source) {
        Tool engine = source.Header switch {
            "Weather" => new Weather(),
            _ => null
        };

        JsonObject properties = [];
        int property = 1;
        while (source.Values.TryGetValue("Property" + property, out string name)) {
            string key;
            JsonNode value;
            if (source.Values.TryGetValue($"Property{property}Description", out string description)) {
                key = "description";
                value = description;
            } else if (source.Values.TryGetValue($"Property{property}Enum", out string enumeration)) {
                key = "description";
                value = new JsonArray([.. Config.GetKeywordList(enumeration).Select(x => (JsonNode)x)]);
            } else {
                property++;
                Logger.Warning($"Unable to parse the {name} property for {source.Header}.");
                continue;
            }
            properties[name] = new JsonObject {
                ["type"] = "string",
                [key] = value
            };
            property++;
        }
        JsonObject jinja = new() {
            ["type"] = "function",
            ["function"] = new JsonObject {
                ["name"] = source["Name"],
                ["description"] = source["Description"],
                ["parameters"] = new JsonObject {
                    ["type"] = "object",
                    ["properties"] = properties,
                    ["required"] = new JsonArray([.. Config.GetKeywordList(source["Required"]).Select(x => (JsonNode)x)])
                }
            }
        };
        return (engine, jinja);
    }
}
