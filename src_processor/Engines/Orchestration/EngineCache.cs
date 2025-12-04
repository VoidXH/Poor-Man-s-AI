using System.Net;

using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Engines.Models;

namespace PoorMansAI.Engines.Orchestration; 

/// <summary>
/// Keeps <see cref="Engine"/>s by certain rules active and reusable. Sends commands through cached instances, switches running software if needed.
/// This class runs the main engine of the software, using the configuration settings.
/// </summary>
public class EngineCache : IDisposable {
    /// <summary>
    /// Rule to keep engines alive by.
    /// </summary>
    public EngineCacheMode Mode {
        get => mode;
        set {
            if (mode == value) {
                return;
            }

            // Shutdowns
            bool slm = (value & EngineCacheMode.SLM) != 0;
            if (chatEngine?.LLM == true || !slm) {
                chatEngine?.Dispose();
                chatEngine = null;
            }

            bool llm = (value & EngineCacheMode.LLM) != 0;
            if (chatEngine?.LLM == false || !llm) {
                chatEngine?.Dispose();
                chatEngine = null;
            }

            bool moa = (value & EngineCacheMode.Image) != 0;
            if (!moa) {
                imageEngine?.Dispose();
                imageEngine = null;
            }

            // Launches
            if (moa) {
                if (imageEngine == null) {
                    imageEngine = new StableDiffusionWebUI();
                    imageEngine.OnProgress += CallOnProgress;
                }
            }

            if (slm && chatEngine == null) {
                chatEngine = SetupLlamaCpp(false);
                chatEngine.OnProgress += CallOnProgress;
            }

            if (llm && chatEngine == null) {
                chatEngine = SetupLlamaCpp(true);
                chatEngine.OnProgress += CallOnProgress;
            }

            mode = value;
            Logger.Info($"Ready in {mode} mode.");
        }
    }
    EngineCacheMode mode;

    /// <summary>
    /// An LLM is running.
    /// </summary>
    public bool CanGenerateText => chatEngine != null;

    /// <summary>
    /// An image generator is running.
    /// </summary>
    public bool CanGenerateImages => imageEngine != null;

    /// <summary>
    /// Reports partial progression and midway states.
    /// </summary>
    public Engine.Progress OnProgress;

    /// <summary>
    /// Authentication cookies to the server.
    /// </summary>
    readonly CookieCollection cookies;

    /// <summary>
    /// Active chatbot instance, could be CPU (SLM mode) or GPU (LLM mode).
    /// </summary>
    LlamaCpp chatEngine;

    /// <summary>
    /// Active image generator, possibly takes up the entire GPU.
    /// </summary>
    StableDiffusionWebUI imageEngine;

    /// <summary>
    /// On launch, the system default mode is used and sent to the server for activation.
    /// </summary>
    public EngineCache(CookieCollection cookies) {
        this.cookies = cookies;
        string GetMode() => HTTP.GET(HTTP.Combine(Config.publicWebserver, "/cmd/models.php?active"), cookies);
        string mode = GetMode();
        while (mode == null) {
            Logger.Warning("Couldn't fetch the active model, retrying...");
            mode = GetMode();
        }
        try {
            Mode = (EngineCacheMode)int.Parse(mode);
        } catch {
            Logger.Error("Invalid login credentials.");
            throw;
        }
    }

    /// <summary>
    /// Initialize llama.cpp with the user configuration for the main LLM runner.
    /// </summary>
    static LlamaCpp SetupLlamaCpp(bool llm) {
        LlamaCppSettings settings = new() {
            Port = Config.llamaCppPort,
            Timeout = Config.chatTimeout,
            Loading = Config.chatLoading,
            Context = Config.chatContext,
            Keep = Config.chatKeep,
            Discard = Config.chatDiscard,
        };

        Dictionary<string, LLModel> models = [];
        foreach (string prefix in Config.ForEachModel()) {
            LLModel model = new(prefix, llm);
            models[model.Name] = model;
        }
        return new(llm, settings, models);
    }

    /// <summary>
    /// Run a <paramref name="command"/> through the corresponding engine.
    /// </summary>
    /// <returns>The output of the generation.</returns>
    public string Generate(Command command) {
        if (command.EngineType == EngineType.Mode) {
            Mode = (EngineCacheMode)int.Parse(command.Prompt);
            HTTP.POST(HTTP.Combine(Config.publicWebserver, "/cmd/models.php"), [
                new KeyValuePair<string, string>("active", command.Prompt)
            ], cookies);
            return "OK";
        }

        return command.EngineType switch {
            EngineType.Chat => chatEngine?.Generate(command) ?? "Chat engine is offline.",
            EngineType.Image => imageEngine?.Generate(command) ?? File.ReadAllText("Data/OfflineImage.txt"),
            _ => null
        };
    }

    /// <summary>
    /// Cancel the current operation.
    /// </summary>
    public void StopGeneration(EngineType type) {
        Engine engine = type switch {
            EngineType.Chat => chatEngine,
            EngineType.Image => imageEngine,
            _ => null
        };
        engine?.StopGeneration();
    }

    /// <inheritdoc/>
    public void Dispose() {
        chatEngine?.Dispose();
        imageEngine?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Allow for lazy subscriptions.
    /// </summary>
    void CallOnProgress(Command command, float progress, string status) => OnProgress?.Invoke(command, progress, status);
}
