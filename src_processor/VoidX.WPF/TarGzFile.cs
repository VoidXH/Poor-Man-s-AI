using System.Formats.Tar;
using System.IO.Compression;

namespace VoidX.WPF;

/// <summary>
/// Processes GZip files that are additionally wrapped in .tar.
/// </summary>
public static class TarGzFile {
    /// <summary>
    /// Extract a .tar.gz file to a target folder.
    /// </summary>
    public static void ExtractToDirectory(string compressed, string targetFolder) {
        using FileStream file = File.OpenRead(compressed);
        using GZipStream gzip = new(file, CompressionMode.Decompress);
        TarFile.ExtractToDirectory(gzip, targetFolder, true);
    }
}
