using System.Runtime.CompilerServices;

namespace PoorMansAI.Engines.Orchestration;

/// <summary>
/// Shorthands for checking if an engine is part of an <see cref="EngineCacheMode"/>.
/// </summary>
public static class EngineCacheModeExtensions {
    /// <summary>
    /// The mode contains a small language model.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSLM(this EngineCacheMode mode) => (mode & EngineCacheMode.SLM) != 0;

    /// <summary>
    /// The mode contains a large language model.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLLM(this EngineCacheMode mode) => (mode & EngineCacheMode.LLM) != 0;

    /// <summary>
    /// The mode contains a text generator.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsChat(this EngineCacheMode mode) => IsSLM(mode) || IsLLM(mode);

    /// <summary>
    /// The mode contains an image generator.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsImage(this EngineCacheMode mode) => (mode & EngineCacheMode.Image) != 0;
}
