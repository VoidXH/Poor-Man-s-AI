using System.Text.Json.Nodes;

using PoorMansAI.Configuration;
using PoorMansAI.Engines.BaseClasses;
using PoorMansAI.Engines.Jinja.BaseClasses;

namespace PoorMansAI.Engines.Jinja;

/// <summary>
/// Draws an image with the currently running <see cref="ImageEngine"/>.
/// </summary>
public sealed class Draw : SandwichTool {
    /// </inheritdoc>
    public override string OutputProperty => "image_prompt";

    /// <inheritdoc/>
    public override string Execute(LlamaCpp caller, JsonNode parameters, Engine.Progress progressReporter) {
        if (!caller.Others.TryGetValue(EngineType.Image, out Engine engine)) {
            return imageEngineNotRunning;
        }
        if (engine is not ImageEngine imageEngine) {
            return imageEngineNotRunning;
        }

        string prompt = parameters[OutputProperty]?.ToString() ??
            throw new ArgumentNullException(OutputProperty, $"The '{OutputProperty}' property is missing in a drawing call.");

        caller.OverrideTimeout(Config.imageGenLoading + Config.imageGenTimeout);
        string base64 = imageEngine.Generate(new(EngineType.Image, prompt));
        parameters[OutputProperty] = $"<img src=\"data:image/png;base64,{base64}\" alt=\"{prompt}\">";
        return base.Execute(caller, parameters, progressReporter);
    }

    readonly static string imageEngineNotRunning = "Shhh! The image generating computer is sleeping and can't work now.";
}
