using PoorMansAI.Engines;
using PoorMansAI.Engines.BaseClasses;
using PoorMansAI.Tests.Utilities;

namespace PoorMansAI.Tests.Engines;

/// <summary>
/// Tests the <see cref="ShellAccess"/> <see cref="Engine"/>.
/// </summary>
[TestClass]
public class ShellAccess_Tests : TestWithContext {
    /// <summary>
    /// Tests that the shell engine executes "echo hello" and captures "hello" in the output.
    /// </summary>
    [TestMethod, Timeout(5000, CooperativeCancellation = true)]
    public async Task ShellAccess_EchoHello_ReturnsHello() {
        TaskCompletionSource<string> completion = new();
        void handler(Command command, float progress, string status) {
            Console.WriteLine(status);
            if (status.Equals("hello", StringComparison.Ordinal)) {
                completion.TrySetResult(status);
            }
        }

        ShellAccess engine = (ShellAccess)ShellAccess.Instance;
        engine.OnProgress += handler;

        if (OperatingSystem.IsMacOS()) {
            engine.Generate(new Command(EngineType.Shell, 0, "printf 'hello\\n'"));
        } else {
            engine.Generate(new Command(EngineType.Shell, 0, "echo hello"));
        }

        completion.Task.Wait(TestContext.CancellationToken);
        engine.OnProgress -= handler;

        Assert.IsTrue(completion.Task.IsCompleted, "Expected the shell to produce output within the timeout.");
        string result = completion.Task.Result;
        Assert.IsTrue(result.Equals("hello", StringComparison.Ordinal), $"Expected output to be \"hello\", got: \"{result}\"");
    }
}
