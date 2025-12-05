namespace PoorMansAI.Configuration;

// Parsed config values related to MoA
partial class Config {
    /// <summary>
    /// Use an LLM to select artists instead of a simple keyword search.
    /// </summary>
    public static readonly bool moaAI = bool.Parse(Values["MoAAI"]);

    /// <summary>
    /// Use this port for MoA (if LLM mode is enabled).
    /// </summary>
    public static readonly ushort moaPort = ushort.Parse(Values["MoAPort"]);

    /// <summary>
    /// This model determines which artist to use.
    /// </summary>
    public static readonly string moaModel = Values["MoAModel"];

    /// <summary>
    /// Load the MoA model to the GPU for faster selection.
    /// </summary>
    public static readonly bool moaGPU = bool.Parse(Values["MoAGPU"]);
}
