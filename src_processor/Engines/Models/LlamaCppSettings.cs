using System.Runtime.InteropServices;

using PoorMansAI.Configuration;

namespace PoorMansAI.Engines.Models;

/// <summary>
/// All settings for the llama.cpp engine with their default values.
/// </summary>
public class LlamaCppSettings {
    /// <summary>
    /// Use the larger models on the GPU instead of the small ones on the CPU.
    /// </summary>
    public bool GPU { get; set; }

    /// <summary>
    /// Open the HTTP endpoints on this port.
    /// </summary>
    public ushort Port { get; set; } = 64100;

    /// <summary>
    /// If chat replies are not done in this many seconds, cancel the generation.
    /// </summary>
    public int Timeout { get; set; } = 90;

    /// <summary>
    /// When switching models, allow this many extra seconds over the normal timeout.
    /// </summary>
    public int Loading { get; set; } = 15;

    /// <summary>
    /// Maximum number of tokens to generate (-1 = infinite).
    /// </summary>
    public int Predict { get; set; } = -1;

    /// <summary>
    /// Maximum context length for prompts.
    /// </summary>
    public int Context { get; set; } = 4096;

    /// <summary>
    /// How much to keep of the initial prompt context for each generation.
    /// </summary>
    public int Keep { get; set; } = 128;

    /// <summary>
    /// How much space to keep at the end of the context window for new messages.
    /// </summary>
    public int Discard { get; set; } = 512;

    /// <summary>
    /// When using models with Multi-Token Prediction, predict this many at once.
    /// </summary>
    public int MTP { get; set; } = 1;

    /// <summary>
    /// Parallel requests chat can serve, larger values take up more RAM.
    /// </summary>
    public int Parallel { get; set; } = 1;

    /// <summary>
    /// Create a settings holder with default values.
    /// </summary>
    public LlamaCppSettings() { }

    /// <summary>
    /// Read the settings for llama.cpp from the main configuration file.
    /// </summary>
    public LlamaCppSettings(bool llm) {
        GPU = llm || RuntimeInformation.IsOSPlatform(OSPlatform.OSX); // Mac is Unified, GPU mode is about 0.5% faster
        Port = Config.llamaCppPort;
        Timeout = Config.chatTimeout;
        Loading = Config.chatLoading;
        Predict = Config.chatPredict;
        Context = llm ? Config.chatContextLLM : Config.chatContextSLM;
        Keep = Config.chatKeep;
        Discard = Config.chatDiscard;
        MTP = Config.chatMTP;
        Parallel = Config.chatParallel;
    }

    /// <summary>
    /// Get the model list from the main configuration file.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, LLModel> GetConfiguredModels() {
        Dictionary<string, LLModel> models = [];
        foreach (string prefix in Config.ForEachModel()) {
            LLModel model = new(prefix, GPU);
            models[model.Name] = model;
        }
        return models;
    }
}
