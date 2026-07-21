namespace PoorMansAI.Engines.Utilities;

/// <summary>
/// Sanitizes agent output by removing debug/prologue lines that are not part of the actual response.
/// </summary>
public class AgentOutputSanitizer {
    /// <summary>
    /// The first line is yet to be processed.
    /// </summary>
    bool firstLine = true;

    /// <summary>
    /// We're in a block to skip, an empty line marks the end of the block.
    /// </summary>
    bool skipUntilEmptyLine;

    /// <summary>
    /// Process a line from agent output. Returns whether the line should be included in the final output.
    /// </summary>
    /// <param name="line">The raw line from standard output.</param>
    /// <returns><see langword="true"/> if the line should be kept; <see langword="false"/> if it should be skipped.</returns>
    public bool ShouldKeepLine(string line) {
        if (firstLine) {
            firstLine = false;
            if (line.Contains("Disabled tools:")) {
                skipUntilEmptyLine = true;
            }
        }

        if (skipUntilEmptyLine) {
            if (string.IsNullOrEmpty(line)) {
                skipUntilEmptyLine = false;
            } else {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Reset the sanitizer state so it can be reused for a new output stream.
    /// </summary>
    public void Reset() {
        firstLine = true;
        skipUntilEmptyLine = false;
    }
}
