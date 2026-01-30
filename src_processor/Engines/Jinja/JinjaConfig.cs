using System.Text;
using System.Text.Json.Nodes;

using VoidX.WPF;

using PoorMansAI.Configuration;

namespace PoorMansAI.Engines.Jinja;

/// <summary>
/// Contains tools available to <see cref="LlamaCpp"/>.
/// </summary>
public class JinjaConfig {
    /// <summary>
    /// The LLM wanted to use a tool.
    /// </summary>
    public bool ToolSelected => usedTool.Length != 0;

    /// <summary>
    /// Tools to attach to API calls.
    /// </summary>
    readonly JsonArray serializedTools = [];

    /// <summary>
    /// Available engines by name.
    /// </summary>
    readonly Dictionary<string, Tool> tools = [];

    /// <summary>
    /// Name of the selected tool.
    /// </summary>
    string usedTool;

    /// <summary>
    /// Accumulates tool usage parameters.
    /// </summary>
    StringBuilder toolParams;

    /// <summary>
    /// Loads tools available to <see cref="LlamaCpp"/> from a config file at the given <paramref name="path"/>.
    /// </summary>
    public JinjaConfig(string path) {
        IniFileBlock[] parsed = IniFile.Parse(path);
        foreach (IniFileBlock tool in parsed) {
            (Tool engine, JsonObject jinja) = Tool.Parse(tool);
            tools[(string)jinja["function"]["name"]] = engine;
            serializedTools.Add(jinja);
        }
    }

    /// <summary>
    /// Add the supported tools to the endpoint API call and reset the tool request accumulator.
    /// </summary>
    public void Attach(JsonObject apiCall) {
        apiCall["tool_choice"] = "auto";
        apiCall["tools"] = serializedTools.DeepClone();
        usedTool = string.Empty;
        toolParams = new();
    }

    /// <summary>
    /// Accumulates tool call request deltas, and when everything is available, runs the tool.
    /// </summary>
    public void ParseToolCalls(JsonNode toolCalls) {
        JsonNode function = toolCalls[0]["function"];
        if (function == null) {
            return;
        }

        JsonNode toolName = function["name"];
        if (toolName != null) {
            usedTool += toolName.ToString();
        }

        JsonNode toolArgs = function["arguments"];
        if (toolArgs != null) {
            toolParams.Append(toolArgs.ToString());
        }
    }

    /// <summary>
    /// Handles the tool launch and progress reporting when all parameters are received.
    /// </summary>
    /// <param name="progressReporter">The final output of the launched tool.</param>
    public string LaunchTool(LlamaCpp caller, Engine.Progress progressReporter) {
        if (!tools.TryGetValue(usedTool, out Tool tool)) {
            Logger.Warning($"Requested tool ({usedTool}) is not configured.");
            return Config.internalErrorMessage;
        }

        JsonNode parameters = JsonNode.Parse(toolParams.ToString());
        return tool.Execute(caller, parameters, progressReporter);
    }
}
