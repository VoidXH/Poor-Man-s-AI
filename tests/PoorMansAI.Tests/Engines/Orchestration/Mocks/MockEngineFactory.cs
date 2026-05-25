using PoorMansAI.Engines.BaseClasses;
using PoorMansAI.Engines.Orchestration;
using PoorMansAI.Tests.Engines.Mocks;

namespace PoorMansAI.Tests.Engines.Orchestration.Mocks;

/// <summary>
/// Mock implementation of <see cref="EngineFactory"/> for unit testing, creating the mock engines in this test project.
/// </summary>
public class MockEngineFactory : EngineFactory {
    /// <inheritdoc/>
    protected override ChatEngine StartSLM() => new MockChatEngine(false);

    /// <inheritdoc/>
    protected override ChatEngine StartLLM() => new MockChatEngine(true);

    /// <inheritdoc/>
    protected override ImageEngine StartImage() => new MockImageEngine();
}
