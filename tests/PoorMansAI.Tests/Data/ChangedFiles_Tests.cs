using PoorMansAI.Data;
using PoorMansAI.Tests.Data.Utilities;

namespace PoorMansAI.Tests.Data;

/// <summary>
/// Tests the <see cref="ChangedFiles"/> class.
/// </summary>
[TestClass]
public class ChangedFiles_Tests {
    /// <summary>
    /// Verifies that <see cref="ChangedFiles.GetChangedFiles"/> returns a single changed file named 'ChangedFile.txt' when the repository has uncommitted changes.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void GetChangedFiles_GitWithChanges_ReturnsSingleChangedFile() {
        TestDataPreparation.AssertChangedGitRepo();
        string[] changed = ChangedFiles.GetChangedFiles(TestDataPreparation.changedGitRepoLocation);
        Assert.HasCount(1, changed, "Expected exactly one changed file.");
        Assert.AreEqual("ChangedFile.txt", changed[0], "Expected the changed file to be named 'ChangedFile.txt'.");
    }
}
