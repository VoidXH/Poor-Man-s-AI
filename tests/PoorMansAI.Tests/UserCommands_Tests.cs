using System.Diagnostics;

using PoorMansAI.Configuration;
using PoorMansAI.Engines;
using PoorMansAI.Tests.Utilities;

namespace PoorMansAI.Tests;

/// <summary>
/// Tests the <see cref="UserCommands"/> class.
/// </summary>
[TestClass]
public class UserCommands_Tests : TestWithContext {
    /// <summary>
    /// The login session used by all tests in this class.
    /// </summary>
    private static UserCommands userCommands;

    /// <summary>
    /// Uses the same login session for all tests in this class.
    /// </summary>
    [ClassInitialize]
    public static void ClassInitialize(TestContext context) => userCommands = new();

    /// <summary>
    /// Verifies that submitting a chat command returns a valid chat response.
    /// </summary>
    [TestMethod, Timeout(10000, CooperativeCancellation = true)]
    public void SubmitChatCommand_ReturnsResponse() => SendChatMessageAndGetResponse("hello", null);

    /// <summary>
    /// Verifies that stopping a running command works and completes within 3 seconds of the first valid reply.
    /// </summary>
    [TestMethod, Timeout(10000, CooperativeCancellation = true)]
    public void StopCommand_StopsWithinTimeLimit() {
        DateTime? firstResponseTime = null;
        SendChatMessageAndGetResponse("Write something long", (id, message) => {
            if (!string.IsNullOrWhiteSpace(message) && message != CommandRunner.ProcessingError) {
                if (firstResponseTime == null) {
                    firstResponseTime = DateTime.UtcNow;
                    if (!userCommands.StopCommand(id)) {
                        Assert.Fail("StopCommand should have succeeded.");
                        return;
                    }
                } else if (firstResponseTime + TimeSpan.FromSeconds(3) > DateTime.UtcNow) {
                    Assert.Fail("StopCommand should have succeeded after 3 seconds.");
                    return;
                }
            }
        });

        Assert.IsNotNull(firstResponseTime, "Should have received at least one response before timing out.");
        Assert.IsTrue(firstResponseTime + TimeSpan.FromSeconds(3) > DateTime.UtcNow, "3 seconds did not remain from the test timeout.");
    }

    /// <summary>
    /// Submits a command and waits for a valid response, returning the result string.
    /// Asserts failure if processing errors occur or a timeout happens.
    /// </summary>
    string SendCommandAndGetResponse(EngineType type, string payload, Action<int, string> onStatus) {
        int commandId = userCommands.SubmitCommand(type, payload);
        Assert.AreNotEqual(-1, commandId, "Command ID should indicate success.");

        while (!TestContext.CancellationToken.IsCancellationRequested) {
            (bool success, int _, string status) = userCommands.CheckCommand(commandId);
            Assert.IsTrue(success, "Check command returned failure.");
            onStatus?.Invoke(commandId, status);

            if (status == CommandRunner.ProcessingError) {
                Assert.Fail("Formatting or processing error.");
            } else if (!string.IsNullOrWhiteSpace(status)) {
                Console.WriteLine(status);
                return status;
            }

            Thread.Sleep(1000);
        }

        Assert.Fail("Timed out waiting for response.");
        return null; // Unreachable, but satisfies compiler
    }

    /// <summary>
    /// Submits a chat command and waits for a valid response, returning the result string.
    /// </summary>
    string SendChatMessageAndGetResponse(string message, Action<int, string> onStatus) {
        string modelKey = Config.GetModelNames().First();
        return SendCommandAndGetResponse(EngineType.Chat, modelKey + "|" + message, (id, message) => {
            if (message == LlamaCpp.ModelNotFound) {
                Assert.Fail("Chat model not found: " + modelKey);
            }

            onStatus?.Invoke(id, message);
        });
    }
}
