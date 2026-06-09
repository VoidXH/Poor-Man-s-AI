using System.Text;

namespace PoorMansAI.Data;

/// <summary>
/// Gets sets of changed files.
/// </summary>
public static class ChangedFiles {
    /// <summary>
    /// Returns all diffs for changed files in a <paramref name="folder"/> in formatted HTML.
    /// </summary>
    public static string GetAllDiffs(string folder) {
        string output = GitUtils.RunGit(folder, "-c core.quotepath=true diff HEAD");
        if (string.IsNullOrWhiteSpace(output)) {
            return "No changes.";
        }

        string encodedOutput = Convert.ToBase64String(Encoding.ASCII.GetBytes(output));
        return string.Format(gitDiffDisplay, encodedOutput);
    }

    /// <summary>
    /// List the changed files in a <paramref name="folder"/>.
    /// </summary>
    public static string[] GetChangedFiles(string folder) {
        string output = GitUtils.RunGit(folder, "diff --name-only HEAD");

        if (string.IsNullOrWhiteSpace(output)) {
            return ["No changes."];
        }
        return [.. output
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line))];
    }

    static readonly string gitDiffDisplay = @"
<link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/diff2html/bundles/css/diff2html.min.css"" />
<script src=""https://cdn.jsdelivr.net/npm/diff2html/bundles/js/diff2html-ui.min.js""></script>
<style>table .d2h-info, table .d2h-ins, table .d2h-del, .d2h-file-header {{ color: black; }}</style>
<div id=""diffDisplay""></div>
<script>
  (function() {{
    const rawDiff = decodeURIComponent(escape(atob('{0}')));
    const targetElement = document.getElementById('diffDisplay');
    const checkReady = setInterval(function() {{
      if (typeof Diff2HtmlUI !== 'undefined') {{
        clearInterval(checkReady);
        const diff2htmlUi = new Diff2HtmlUI(targetElement, rawDiff, {{
          drawFileList: true,
          matching: 'lines'
        }});
        diff2htmlUi.draw();
      }}
    }}, 50);
  }})();
</script>";
}
