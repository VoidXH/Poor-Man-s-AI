using System.Net;

using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Engines.BaseClasses;

namespace PoorMansAI.Engines.Orchestration;

/// <summary>
/// Keeps <see cref="Engine"/>s by certain rules active and reusable. Sends commands through cached instances, switches running software if needed.
/// This class runs the main engine of the software, using the configuration settings.
/// </summary>
public class EngineCache : IDisposable {
    /// <summary>
    /// The logic that initializes the running engines.
    /// </summary>
    public EngineFactory Factory { get; set; } = new EngineFactory();

    /// <summary>
    /// Rule to keep engines alive by.
    /// </summary>
    public EngineCacheMode Mode {
        get => mode;
        set {
            if (mode != value && Factory.Modify(engines, value, CallOnProgress)) {
                mode = value;
                Logger.Info($"Ready in {mode} mode.");
            }
        }
    }
    EngineCacheMode mode;

    /// <summary>
    /// Reports partial progression and midway states.
    /// </summary>
    public Engine.Progress OnProgress;

    /// <summary>
    /// Active engine instances.
    /// </summary>
    public IReadOnlyDictionary<EngineType, Engine> Engines => engines;
    readonly Dictionary<EngineType, Engine> engines = [];

    /// <summary>
    /// Authentication cookies to the server.
    /// </summary>
    readonly CookieCollection cookies;

    /// <summary>
    /// Launch an engine selector with a predefined <paramref name="mode"/>.
    /// </summary>
    public EngineCache(EngineCacheMode mode) => Mode = mode;

    /// <summary>
    /// On launch, the system default mode is used and sent to the server for activation.
    /// </summary>
    public EngineCache(CookieCollection cookies) {
        this.cookies = cookies;
        string GetMode() => HTTP.GET(HTTP.Combine(Config.publicWebserver, "/cmd/models.php?active"), cookies);
        string mode = GetMode();
        while (mode == null) {
            Logger.Warning("Couldn't fetch the active model, retrying...");
            Thread.Sleep(1000);
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
            EngineType.Chat => engines.TryGetValue(EngineType.Chat, out Engine engine) ?
                engine.Generate(command) :
                "Chat engine is offline.",
            EngineType.Image => engines.TryGetValue(EngineType.Image, out Engine engine) ?
                engine.Generate(command) :
                File.ReadAllText("Data/OfflineImage.txt"),
            _ => null
        };
    }

    /// <summary>
    /// Cancel the current operation.
    /// </summary>
    public void StopGeneration(EngineType type) {
        if (engines.TryGetValue(type, out Engine engine)) {
            engine?.StopGeneration();
        }
    }

    /// <inheritdoc/>
    public void Dispose() {
        foreach (Engine engine in engines.Values) {
            engine.Dispose();
        }
        engines.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Allow for lazy subscriptions.
    /// </summary>
    void CallOnProgress(Command command, float progress, string status) => OnProgress?.Invoke(command, progress, status);
}
