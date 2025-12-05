using System.Text;

using PoorMansAI.Configuration;
using PoorMansAI.Engines;
using PoorMansAI.Engines.Models;
using VoidX.WPF;

namespace PoorMansAI.NewTech.VoidMoA;

/// <summary>
/// Assigns a model by the decision of an LLM. Handles a lot of cases better than <see cref="KeywordBasedPromptTransformer"/>, but can be very wrong in some cases.
/// Additionally, the system requirements are significant and can be slow.
/// </summary>
public class LLMBasedPromptTransformer : PromptTransformer {
    /// <summary>
    /// Handles the LLM assigned to MoA.
    /// </summary>
    readonly LlamaCpp engine;

    /// <summary>
    /// Assigns a model by the decision of an LLM.
    /// </summary>
    public LLMBasedPromptTransformer() {
        LlamaCppSettings settings = new() {
            GPU = Config.moaGPU,
            Port = Config.moaPort,
            Timeout = Config.imageGenTimeout / 4,
            Loading = Config.imageGenLoading,
            Predict = 10,
        };
        engine = new(settings, new Dictionary<string, LLModel>() {
            [modelId] = new LLModel(Config.moaModel, "You are a helpful assistant.")
        });
    }

    /// <inheritdoc/>
    protected override ArtistConfiguration AssignModel(string prompt, string[] keywords, ref string[] selectors) {
        ArtistConfiguration[] artists = Config.artistConfigs;
        StringBuilder enginePrompt = new(modelId);
        enginePrompt.AppendLine("|Your job is selecting a model number for an image generator prompt. Models:")
            .AppendLine("0: default, generic, other");
        for (int i = 0; i < artists.Length; i++) {
            enginePrompt.Append(i + 1).Append(": ").AppendLine(string.Join(", ", artists[i].keywords));
        }
        enginePrompt.Append("Reply with a single number selecting a model for the following prompt: ")
            .AppendLine(prompt);
        string selection = engine.Generate(new Command(EngineType.Chat, enginePrompt.ToString()));
        if (int.TryParse(selection, out int result)) {
            return result < 1 || result > artists.Length ?
                null :
                artists[result - 1];
        } else {
            Logger.Warning($"MoA selecting LLM didn't reply an artist index but: \"{selection}\"");
            return null;
        }
    }

    /// <inheritdoc/>
    public override void Dispose() {
        engine.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Pair the model and prompt with this identifier.
    /// </summary>
    const string modelId = "MoA";
}
