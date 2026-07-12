using PoorMansAI.Engines;
using PoorMansAI.Engines.Models;
using PoorMansAI.Tests.Data.Utilities;
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
    public void Generate_CommandEchoingHello_ReturnsHelloOutput() => RunEchoTest("hello", "hello");

    /// <summary>
    /// Verifies that <see cref="AgentEngine.Generate"/> strips the extra command and runs it.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Generate_ExtraCommandGitStatus_ReturnsGitStatus() {
        TestDataPreparation.AssertChangedGitRepo();
        RunEchoTest(TestDataPreparation.changedGitRepoLocation + "|[GitStatus]", "ChangedFile.txt");
    }

    /// <summary>
    /// Verifies that <see cref="AgentEngine.Generate"/> strips the extra command and runs the prompt after it.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Generate_ExtraCommandGitStatusWithEcho_ReturnsEcho() {
        TestDataPreparation.AssertChangedGitRepo();
        RunEchoTest(TestDataPreparation.changedGitRepoLocation + "|[GitStatus]hello", $"ChangedFile.txt{Environment.NewLine}hello");
    }

    /// <summary>
    /// Verifies that <see cref="AgentEngine.Generate"/> does not strip the extra command if it's not an extra command.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Generate_InvalidExtraCommandInEcho_ReturnsCommandAndEcho() {
        TestDataPreparation.AssertChangedGitRepo();
        RunEchoTest(TestDataPreparation.changedGitRepoLocation + "|[Invalid]hello", $"[Invalid]hello");
    }

    /// <summary>
    /// Run the <see cref="AgentEngine"/> with the <paramref name="prompt"/> sent into an echo command, and check if the echo contains an <paramref name="expectedReply"/>.
    /// </summary>
    static void RunEchoTest(string prompt, string expectedReply) {
        using AgentEngine engine = new(new Dictionary<string, AgentModel> {
            ["Test"] = new AgentModel("Agent1") {
                Name = "Test",
                Command = "echo {{PROMPT}}"
            }
        });
        Command command = new(EngineType.Agent, prompt);
        string result = engine.Generate(command);

        Assert.IsNotNull(result);
        Assert.Contains(expectedReply, result, $"Expected output to contain '{expectedReply}', but got: {result}");
    }
}
