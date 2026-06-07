using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Engines.BaseClasses;

namespace PoorMansAI.Engines.Orchestration;

/// <summary>
/// Handles running engine instances by <see cref="EngineCacheMode"/>.
/// The default implementation handles the engine types supported out of the box, but dependency injection can add new engines to the <see cref="EngineCache"/>.
/// </summary>
public class EngineFactory {
    /// <summary>
    /// Adjusts an existing dictionary of engines to match the requested <paramref name="mode"/>, shutting down removed engines and starting the new ones needed.
    /// </summary>
    /// <returns>If changes were made.</returns>
    public bool Modify(Dictionary<EngineType, Engine> engines, EngineCacheMode mode, Engine.Progress onProgress) =>
        Shutdown(engines, mode) && Startup(engines, mode, onProgress);

    /// <summary>
    /// Stop <paramref name="engines"/> that are not in the requested <paramref name="mode"/>.
    /// </summary>
    /// <returns>If no errors were found and the modification of running <paramref name="engines"/> can continue.</returns>
    static bool Shutdown(Dictionary<EngineType, Engine> engines, EngineCacheMode mode) {
        ChatEngine activeChat = engines.TryGetValue(EngineType.Chat, out Engine engine) ? engine as ChatEngine : null;

        if (mode.IsSLM() && mode.IsLLM()) {
            Logger.Error("Cannot enable both SLM and LLM modes at the same time.");
            return false;
        }

        if (activeChat != null && ((!mode.IsSLM() || activeChat.GPU == true) || (!mode.IsLLM() || activeChat.GPU == false))) {
            activeChat.Dispose();
            engines.Remove(EngineType.Chat);
        }
        if (!mode.IsImage() && engines.TryGetValue(EngineType.Image, out Engine imageEngineToRemove)) {
            imageEngineToRemove.Dispose();
            engines.Remove(EngineType.Image);
        }

        return true;
    }

    /// <summary>
    /// Start a new chat engine with a small language model.
    /// </summary>
    protected virtual ChatEngine StartSLM() => new LlamaCpp(new(false));

    /// <summary>
    /// Start a new chat engine with a large language model.
    /// </summary>
    protected virtual ChatEngine StartLLM() => new LlamaCpp(new(true));

    /// <summary>
    /// Start a new image generator engine.
    /// </summary>
    protected virtual ImageEngine StartImage() => new StableDiffusionWebUI();

    /// <summary>
    /// Start <paramref name="engines"/> that are in the requested <paramref name="mode"/> and are not already running.
    /// </summary>
    /// <returns>If no errors were found and the requested engines were started.</returns>
    bool Startup(Dictionary<EngineType, Engine> engines, EngineCacheMode mode, Engine.Progress onProgress) {
        if (mode.IsImage() && !engines.ContainsKey(EngineType.Image) && Config.chatWeight >= 0) {
            ImageEngine imageEngine = StartImage();
            imageEngine.Others = engines;
            imageEngine.OnProgress += onProgress;
            engines[EngineType.Image] = imageEngine;
        }

        if (mode.IsChat() && !engines.ContainsKey(EngineType.Chat) && Config.imageGenWeight >= 1) {
            ChatEngine chatEngine = mode.IsLLM() ? StartLLM() : StartSLM();
            chatEngine.Others = engines;
            chatEngine.OnProgress += onProgress;
            engines[EngineType.Chat] = chatEngine;
        }

        if (!engines.ContainsKey(EngineType.Agent) && Config.agentWeight >= 0) {
            AgentEngine agentEngine = new(new(true)) {
                Others = engines
            };
            agentEngine.OnProgress += onProgress;
            engines[EngineType.Agent] = agentEngine;
            Logger.Info("Agentic command handling enabled.");
        }

        return true;
    }
}
