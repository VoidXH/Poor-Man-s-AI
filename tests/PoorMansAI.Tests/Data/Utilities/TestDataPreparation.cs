using PoorMansAI.Tests.Utilities;

namespace PoorMansAI.Tests.Data.Utilities;

/// <summary>
/// Creates test data folder structures.
/// </summary>
static class TestDataPreparation {
    /// <summary>
    /// Changed git repo location.
    /// </summary>
    public static readonly string changedGitRepoLocation = Path.Combine(Constants.testData, "GitWithChanges");

    /// <summary>
    /// Makes sure the test repository under tests/TestData/GitWithChanges exists with one changed and one unchanged file.
    /// </summary>
    public static void AssertChangedGitRepo() {
        string changedFilePath = Path.Combine(changedGitRepoLocation, "ChangedFile.txt");
        if (File.Exists(changedFilePath)) {
            return;
        }

        Directory.CreateDirectory(changedGitRepoLocation);
        File.WriteAllText(changedFilePath, "version 1");
        File.WriteAllText(Path.Combine(changedGitRepoLocation, "UnchangedFile.txt"), "version 1");
        GitUtils.RunGit(changedGitRepoLocation, "init");
        GitUtils.RunGit(changedGitRepoLocation, $"add {Path.GetFileName(changedFilePath)}");
        GitUtils.RunGit(changedGitRepoLocation, "commit -m \"Initial commit\"");
        File.WriteAllText(changedFilePath, "version 2");
    }
}
