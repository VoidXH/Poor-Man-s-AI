namespace PoorMansAI.NewTech.ContextDocTree {
    /// <summary>
    /// If there are too many files that would be parsed for the Context Doc Tree system, this exception is thrown,
    /// and a fallback to searching the file system for each prompt is used.
    /// </summary>
    public class TooManyFilesException : Exception { }
}