using PoorMansAI.Engines.BaseClasses;

namespace PoorMansAI.Tests.Engines.Mocks;

/// <summary>
/// Mock implementation of <see cref="ChatEngine"/> for unit testing.
/// </summary>
public class MockChatEngine(bool gpu) : ChatEngine {
    /// <inheritdoc/>
    public override bool GPU { get; } = gpu;

    /// <inheritdoc/>
    public override string Generate(Command command) {
        UpdateProgress(command, 1f, "Done");
        return command.Prompt;
    }

    /// <inheritdoc/>
    public override void StopGeneration() { }

    /// <inheritdoc/>
    public override void Dispose() => GC.SuppressFinalize(this);
}
