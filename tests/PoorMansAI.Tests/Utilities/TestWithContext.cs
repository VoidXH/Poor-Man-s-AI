namespace PoorMansAI.Tests.Utilities;

/// <summary>
/// A test where MSTest can set <see cref="TestContext"/> for cooperative cancellation support.
/// </summary>
public abstract class TestWithContext {
    /// <summary>
    /// Test environment context provided by MSTest, which includes a cancellation token that can be used for cooperative cancellation in tests.
    /// </summary>
    public TestContext TestContext { get; set; }
}
