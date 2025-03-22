using System.IO.Compression;

using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;

using VoidX.WPF;

namespace PoorMansAI.Configuration {
    /// <summary>
    /// Downloads all files required by the <see cref="Config"/> file.
    /// </summary>
    public class Downloader {
        /// <summary>
        /// Downloads all files required by the <see cref="Config"/> file.
        /// </summary>
        public static void Prepare() {
            bool prepTextSent = false;
            PrepareLlamaCpp(ref prepTextSent);
            PrepareStableDiffusionWebUI(ref prepTextSent);
            PrepareLLMs(ref prepTextSent);
            PrepareArtists(ref prepTextSent);
        }

        /// <summary>
        /// Downloads and unpacks llama.cpp versions.
        /// </summary>
        static void PrepareLlamaCpp(ref bool prepTextSent) {
            PrepareLlamaCppSingle(Config.llamaCppCPURoot, Config.llamaCppCPUDownload, ref prepTextSent);
            PrepareLlamaCppSingle(Config.llamaCppGPURoot, Config.llamaCppGPUDownload, ref prepTextSent);
            if (OperatingSystem.IsWindows()) {
                string successMarker = Path.Combine(Config.llamaCppGPURoot, "_cudart present.txt");
                if (!File.Exists(successMarker)) {
                    string tempFile = Path.Combine(Config.llamaCppGPURoot, "cudart.zip");
                    if (!File.Exists(tempFile)) {
                        CheckFile(Config.llamaCppCUDADownload, tempFile, ref prepTextSent);
                    }
                    Logger.Info($"Extracting {Path.GetFileName(tempFile)}...");
                    ZipFile.ExtractToDirectory(tempFile, Config.llamaCppGPURoot);
                    File.Delete(tempFile);
                    File.Create(successMarker).Dispose();
                }
            }
        }

        /// <summary>
        /// Downloads and unpacks llama.cpp for a single target device (CPU or GPU).
        /// </summary>
        /// <param name="bin">Folder to extract the binaries to</param>
        /// <param name="download">URL of the build</param>
        /// <param name="prepTextSent">Downloads needed text was displayed to user</param>
        static void PrepareLlamaCppSingle(string bin, string download, ref bool prepTextSent) {
            if (!Directory.Exists(bin)) {
                Directory.CreateDirectory(bin);
            }
            string tempFile = Path.Combine(bin, "llama.zip");
            if (Directory.Exists(bin) && (File.Exists(tempFile) || (Directory.GetFiles(bin).Length + Directory.GetDirectories(bin).Length) == 0)) {
                if (!File.Exists(tempFile)) {
                    CheckFile(download, tempFile, ref prepTextSent);
                }
                Logger.Info($"Extracting {Path.GetFileName(tempFile)}...");
                ZipFile.ExtractToDirectory(tempFile, bin);
                File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Downloads and unpacks Stable Diffusion WebUI.
        /// </summary>
        static void PrepareStableDiffusionWebUI(ref bool prepTextSent) {
            string root = Config.webUIRoot;
            if (!Directory.Exists(root)) {
                Directory.CreateDirectory(root);
            }
            string tempFile = Path.Combine(root, "Release.7z");
            if (Directory.Exists(root) && (File.Exists(tempFile) || Directory.GetDirectories(root).Length == 0)) {
                if (!File.Exists(tempFile)) {
                    CheckFile(Config.webUIDownload, tempFile, ref prepTextSent);
                }
                Extract(tempFile, root);
                File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Downloads models required for chatting.
        /// </summary>
        static void PrepareLLMs(ref bool prepTextSent) {
            Directory.CreateDirectory(Config.models);
            foreach (string prefix in Config.ForEachModel()) {
                CheckLLM(Config.Values[prefix + "SLM"], ref prepTextSent);
                CheckLLM(Config.Values[prefix + "LLM"], ref prepTextSent);
            }
        }

        /// <summary>
        /// Downloads models required for image generation.
        /// </summary>
        static void PrepareArtists(ref bool prepTextSent) {
            Directory.CreateDirectory(Config.artists);
            Directory.CreateDirectory(Config.embeddings);
            CheckSafetensor(Config.defaultArtist, ref prepTextSent);
            GetNegatives("DefaultNegative", ref prepTextSent);
            ArtistConfiguration[] artists = Config.artistConfigs;
            for (int i = 0; i < artists.Length; i++) {
                CheckSafetensor(artists[i].url, ref prepTextSent);
                GetNegatives($"Artist{i + 1}Negative", ref prepTextSent);
            }
        }

        /// <summary>
        /// Check if a file was already downloaded, and download it if it wasn't.
        /// </summary>
        static void CheckFile(string url, string path, ref bool prepTextSent) {
            if (File.Exists(path)) {
                return;
            }

            if (!prepTextSent) {
                Logger.Info("Some model files have to be downloaded before launch.");
                prepTextSent = true;
            }
            Download(url, path).Wait();
        }

        /// <summary>
        /// Check if an LLM was already downloaded, and download it if it wasn't.
        /// </summary>
        static void CheckLLM(string url, ref bool prepTextSent) =>
            CheckFile(url, Path.Combine(Config.models, Path.GetFileName(url)), ref prepTextSent);

        /// <summary>
        /// Check if a negative embedding was already downloaded, and download it if it wasn't.
        /// </summary>
        static void CheckPT(string url, ref bool prepTextSent) =>
            CheckFile(url, Path.Combine(Config.embeddings, Config.GetPTFilename(url)), ref prepTextSent);

        /// <summary>
        /// Check if a stable diffusion checkpoint was already downloaded, and download it if it wasn't.
        /// </summary>
        static void CheckSafetensor(string url, ref bool prepTextSent) =>
            CheckFile(url, Path.Combine(Config.artists, Config.GetSafetensorFilename(url)), ref prepTextSent);

        /// <summary>
        /// Download the negative embeddings if a negative prompt needs it.
        /// </summary>
        static void GetNegatives(string key, ref bool prepTextSent) {
            string[] negatives = Config.ReadList(key);
            for (int j = 0; j < negatives.Length; j++) {
                if (negatives[j].StartsWith("http")) {
                    CheckPT(negatives[j], ref prepTextSent);
                }
            }
        }

        /// <summary>
        /// Get a file from a <paramref name="url"/> and save it to a target <paramref name="path"/>.
        /// </summary>
        static async Task Download(string url, string path) {
            string fileName = Path.GetFileName(path);
            Logger.Info($"Downloading {fileName}:");
            const int bufferSize = 8192;
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode) {
                Logger.Error($"Failed to download {fileName}. HTTP Status: {response.StatusCode}");
                return;
            }

            long? totalBytes = response.Content.Headers.ContentLength;
            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
            using (FileStream file = new(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true)) {
                var buffer = new byte[bufferSize];
                long totalRead = 0;
                int bytesRead;
                DateTime nextUpdate = default;
                while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0) {
                    await file.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalRead += bytesRead;
                    if (totalBytes.HasValue && (nextUpdate < DateTime.Now || totalRead == totalBytes)) {
                        nextUpdate = DateTime.Now + TimeSpan.FromSeconds(1);
                        ProgressBar(totalRead, totalBytes.Value);
                    }
                }
            }

            Console.WriteLine();
            Logger.Info("Download completed.");
        }

        /// <summary>
        /// Extract an <paramref name="archive"/> file to an <paramref name="output"/> directory.
        /// </summary>
        static void Extract(string archive, string output) {
            Logger.Info($"Extracting {Path.GetFileName(archive)}...");
            using SevenZipArchive release = SevenZipArchive.Open(archive);
            using IReader reader = release.ExtractAllEntries();

            ExtractionOptions options = new() {
                ExtractFullPath = true,
                Overwrite = true
            };
            DateTime nextUpdate = default;
            int extracted = 0,
                count = release.Entries.Count;
            while (reader.MoveToNextEntry()) {
                IEntry entry = reader.Entry;
                string path = Path.Combine(output, entry.Key);
                if (File.Exists(path) && new FileInfo(path).Length == entry.Size) {
                    continue;
                }
                try {
                    reader.WriteEntryToDirectory(output, options);
                } catch {
                    if (!File.Exists(path) || new FileInfo(path).Length != entry.Size) {
                        Logger.Warning($"Couldn't extract {entry.Key}.");
                    }
                }

                if (nextUpdate < DateTime.Now || ++extracted == count) {
                    nextUpdate = DateTime.Now + TimeSpan.FromSeconds(1);
                    ProgressBar(extracted, count);
                }
            }
            Console.WriteLine();
            Logger.Info("Extraction completed.");
        }

        /// <summary>
        /// Display or update the progress bar with the percentage of the <paramref name="current"/> entry to the <paramref name="total"/> number of entries.
        /// </summary>
        static void ProgressBar(long current, long total) {
            double progress = (double)current / total * 100;
            Console.Write($"\r[{new string('#', (int)progress / 2)}{new string(' ', 50 - (int)progress / 2)}] {progress:F1}%");
        }
    }
}