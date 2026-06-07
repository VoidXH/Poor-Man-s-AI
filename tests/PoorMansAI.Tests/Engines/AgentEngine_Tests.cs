using PoorMansAI.Engines;
using PoorMansAI.Engines.Models;
using PoorMansAI.Tests.Utilities;

namespace PoorMansAI.Tests.Engines;

/// <summary>
/// Tests the <see cref="AgentEngine"/> class.
/// </summary>
[TestClass]
public class AgentEngine_Tests : TestWithContext {
    /// <summary>
    /// Verifies that <see cref="AgentEngine.Generate"/> runs a simple command and returns its output.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Generate_CommandEchoingHello_ReturnsHelloOutput() {
        const string echo = "hello";
        AgentSettings settings = new(false) {
            Timeout = 10,
            Command = "cmd /c echo " + echo
        };

        AgentEngine engine = new(settings);
        Command command = new(EngineType.Agent, "hello");
        string result = engine.Generate(command);

        Assert.IsNotNull(result);
        Assert.Contains(echo, result, $"Expected output to contain '{echo}', but got: {result}");
    }
}
