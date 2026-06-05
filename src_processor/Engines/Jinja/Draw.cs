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
    public override string Execute(LlamaCpp caller, JsonNode parameters, Command command, Engine.Progress progressReporter) {
        if (!caller.Others.TryGetValue(EngineType.Image, out Engine engine)) {
            return imageEngineNotRunning;
        }
        if (engine is not ImageEngine imageEngine) {
            return imageEngineNotRunning;
        }

        string prompt = parameters[OutputProperty]?.ToString() ??
            throw new ArgumentNullException(OutputProperty, $"The '{OutputProperty}' property is missing in a drawing call.");
        caller.OverrideTimeout(Config.imageGenLoading + Config.imageGenTimeout);

        void UpdateProgress(Command command, float progress, string status) => progressReporter.Invoke(command, .5f, $"Generating image({progress:0%})...");

        imageEngine.OnProgress += UpdateProgress;
        string base64;
        try {
            base64 = imageEngine.Generate(new(EngineType.Image, prompt));
        } catch {
            throw;
        } finally {
            imageEngine.OnProgress -= UpdateProgress;
        }

        parameters[OutputProperty] = $"<img src=\"data:image/png;base64,{base64}\" alt=\"{prompt}\">";
        return base.Execute(caller, parameters, command, progressReporter);
    }

    readonly static string imageEngineNotRunning = "Shhh! The image generating computer is sleeping and can't work now.";
}
