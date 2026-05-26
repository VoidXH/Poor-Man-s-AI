using PoorMansAI.Engines.Orchestration;
using PoorMansAI.Tests.Engines.Orchestration.Mocks;

namespace PoorMansAI.Tests.Engines.Utilities;

/// <summary>
/// Provides utility methods for creating test-ready <see cref="EngineCache"/> instances.
/// </summary>
internal static class EngineCacheUtils {
    /// <summary>
    /// Creates a test-ready, engine-mocked <see cref="EngineCache"/> in offline mode for testing.
    /// </summary>
    internal static EngineCache CreateEngineCache() => new(EngineCacheMode.Offline) {
        Factory = new MockEngineFactory()
    };
}
