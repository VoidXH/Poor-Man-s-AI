using PoorMansAI.Data;

namespace PoorMansAI.Tests.Data;

/// <summary>
/// Tests the <see cref="GitIgnoreParser"/> class.
/// </summary>
[TestClass]
public class GitIgnoreParser_Tests {
    /// <summary>
    /// Verifies that ** pattern matches all files recursively.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Pattern_DoubleStar_MatchesAllFiles() {
        TestPattern("**", "test.txt", true);
        TestPattern("**", "folder/test.txt", true);
        TestPattern("**", "folder/sub/test.txt", true);
    }

    /// <summary>
    /// Verifies that **/bin/ matches bin directories at any level.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Pattern_DoubleStarSlashBin_MatchesBinDirectories() {
        TestPattern("**/bin/", "bin/", true);
        TestPattern("**/bin/", "src/bin/", true);
        TestPattern("**/bin/", "src/bin/debug/", true);
        TestPattern("**/bin/", "src/debug/", false);
    }

    /// <summary>
    /// Verifies that **/ matches any folder recursively (directories only).
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Pattern_DoubleStarSlash_MatchesAnyFolder() {
        TestPattern("**/", "folder/", true);
        TestPattern("**/", "folder/subfolder/", true);
        TestPattern("**/", "folder/subfolder/file.txt", false); // Directory-only pattern doesn't match files
    }

    /// <summary>
    /// Verifies that * matches files in any directory.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Pattern_Star_MatchesAnyFile() {
        TestPattern("*", "test.txt", true);
        TestPattern("*", "folder/test.txt", true);
        TestPattern("*", "folder/subfolder/test.txt", true);
    }

    /// <summary>
    /// Verifies that *.txt matches .txt files anywhere (default recursive behavior).
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Pattern_TxtExtension_MatchesRecursively() {
        TestPattern("*.txt", "test.txt", true);
        TestPattern("*.txt", "folder/test.txt", true);
        TestPattern("*.txt", "folder/subfolder/test.txt", true);
    }

    /// <summary>
    /// Verifies that /*.txt only matches at root level.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Pattern_RootTxtExtension_MatchesOnlyAtRoot() {
        TestPattern("/*.txt", "test.txt", true);
        TestPattern("/*.txt", "folder/test.txt", false);
        TestPattern("/*.txt", "folder/subfolder/test.txt", false);
    }

    /// <summary>
    /// Verifies that leading slash anchors to root directory.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Pattern_LeadingSlash_AnchorsToRoot() {
        TestPattern("/test.txt", "test.txt", true);
        TestPattern("/test.txt", "folder/test.txt", false);
    }

    /// <summary>
    /// Verifies that directory-only patterns (ending with /) only match directories.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Pattern_DirectoryOnly_MatchesOnlyDirectories() {
        TestPattern("bin/", "bin/", true);
        TestPattern("bin/", "bin/file.txt", false);
    }

    /// <summary>
    /// Verifies that negation patterns work correctly.
    /// </summary>
    [TestMethod, Timeout(1000)]
    public void Pattern_Negation_Works() {
        TestPattern("*.txt\n!important.txt", "test.txt", true);
        TestPattern("*.txt\n!important.txt", "important.txt", false);
    }

    static void TestPattern(string pattern, string path, bool expectedIgnored) {
        string testDir = Path.Combine(Path.GetTempPath(), "GitIgnoreTest_" + Guid.NewGuid().ToString("N")[..8]);
        try {
            Directory.CreateDirectory(testDir);
            string gitignorePath = Path.Combine(testDir, ".gitignore");
            File.WriteAllText(gitignorePath, pattern);

            bool isDirectory = path.EndsWith('/');
            string testPath = Path.Combine(testDir, path.TrimEnd('/').Replace('/', Path.DirectorySeparatorChar));

            if (isDirectory) {
                Directory.CreateDirectory(testPath);
            } else {
                Directory.CreateDirectory(Path.GetDirectoryName(testPath)!);
                File.WriteAllText(testPath, "test");
            }

            var parser = new GitIgnoreParser(testDir);
            bool ignored = parser.IsIgnored(testPath, testDir);

            Assert.AreEqual(expectedIgnored, ignored, $"Pattern: '{pattern}', Path: '{path}'");
        } finally {
            try { Directory.Delete(testDir, true); } catch { }
        }
    }
}
