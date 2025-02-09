namespace PoorMansAI.Engines {
    /// <summary>
    /// Possible AI engines to run on this system.
    /// </summary>
    public enum EngineType {
        /// <summary>
        /// Special reserved command to change the running engine.
        /// </summary>
        Mode,
        /// <summary>
        /// Large language model chatbot.
        /// </summary>
        Chat,
        /// <summary>
        /// Image generator.
        /// </summary>
        Image
    }
}