namespace PoorMansAI.Configuration {
    // Parsed config values related to image generation
    partial class Config {
        /// <summary>
        /// Where AUTOMATIC1111's Stable Diffusion WebUI is unpacked to (has system and webui subfolders).
        /// </summary>
        public static readonly string webUIRoot = Values["WebUIRoot"];

        /// <summary>
        /// Stable Diffusion WebUI release build download path.
        /// </summary>
        public static readonly string webUIDownload = Values["WebUIDownload"];

        /// <summary>
        /// Use this port for image generation.
        /// </summary>
        public static readonly string webUIPort = Values["WebUIPort"];

        /// <summary>
        /// If images are not done in this many seconds, cancel the generation.
        /// </summary>
        public static readonly int imageGenTimeout = int.Parse(Values["ImageGenTimeout"]);

        /// <summary>
        /// Number of times the image is refined for better results.
        /// </summary>
        public static readonly int imageGenSteps = int.Parse(Values["ImageGenSteps"]);

        /// <summary>
        /// How close the prompts are followed, read your models' recommendation.
        /// </summary>
        public static readonly int imageGenGuidance = int.Parse(Values["ImageGenGuidance"]);

        /// <summary>
        /// Base image size for non-HD generations, width.
        /// </summary>
        public static readonly int imageSizeW = ResX("ImageSize");

        /// <summary>
        /// Base image size for non-HD generations, width.
        /// </summary>
        public static readonly int imageSizeH = ResY("ImageSize");

        /// <summary>
        /// Image size for horizontal/landscape non-HD generations, width.
        /// </summary>
        public static readonly int imageSizeHW = ResX("ImageSizeH");

        /// <summary>
        /// Image size for horizontal/landscape non-HD generations, width.
        /// </summary>
        public static readonly int imageSizeHH = ResY("ImageSizeH");

        /// <summary>
        /// Image size for vertical/portrait non-HD generations, width.
        /// </summary>
        public static readonly int imageSizeVW = ResX("ImageSizeV");

        /// <summary>
        /// Image size for vertical/portrait non-HD generations, width.
        /// </summary>
        public static readonly int imageSizeVH = ResY("ImageSizeV");

        /// <summary>
        /// Base image size for HD generations, width.
        /// </summary>
        public static readonly int imageSizeHDW = ResX("ImageSizeHD");

        /// <summary>
        /// Base image size for HD generations, width.
        /// </summary>
        public static readonly int imageSizeHDH = ResY("ImageSizeHD");

        /// <summary>
        /// Image size for horizontal/landscape HD generations, width.
        /// </summary>
        public static readonly int imageSizeHDHW = ResX("ImageSizeHDH");

        /// <summary>
        /// Image size for horizontal/landscape HD generations, width.
        /// </summary>
        public static readonly int imageSizeHDHH = ResY("ImageSizeHDH");

        /// <summary>
        /// Image size for vertical/portrait HD generations, width.
        /// </summary>
        public static readonly int imageSizeHDVW = ResX("ImageSizeHDV");

        /// <summary>
        /// Image size for vertical/portrait HD generations, width.
        /// </summary>
        public static readonly int imageSizeHDVH = ResY("ImageSizeHDV");

        /// <summary>
        /// Keywords for making the image horizontal/landscape.
        /// </summary>
        public static readonly string[] hKeywords = ReadKeywordList("HKeywords");

        /// <summary>
        /// Selectors for making the image horizontal/landscape.
        /// </summary>
        public static readonly string[] hSelectors = ReadKeywordList("HSelectors");

        /// <summary>
        /// Keywords for making the image vertical/portrait.
        /// </summary>
        public static readonly string[] vKeywords = ReadKeywordList("VKeywords");

        /// <summary>
        /// Selectors for making the image vertical/portrait.
        /// </summary>
        public static readonly string[] vSelectors = ReadKeywordList("VSelectors");

        /// <summary>
        /// Keywords for making the image HD.
        /// </summary>
        public static readonly string[] hdKeywords = ReadKeywordList("HDKeywords");

        /// <summary>
        /// Selectors for making the image HD.
        /// </summary>
        public static readonly string[] hdSelectors = ReadKeywordList("HDSelectors");

        /// <summary>
        /// Stable Diffusion checkpoints are stored in this folder.
        /// </summary>
        public static readonly string artists = Values["Artists"];

        /// <summary>
        /// Stable Diffusion embeddings are stored in this folder.
        /// </summary>
        public static readonly string embeddings = Values["Embeddings"];

        /// <summary>
        /// Default checkpoint to use for image generation.
        /// </summary>
        public static readonly string defaultArtist = Values["DefaultArtist"];

        /// <summary>
        /// Negative embeddings to be used in conjunction with the <see cref="defaultArtist"/>.
        /// </summary>
        public static readonly string defaultNegative = ArtistConfiguration.ReadNegative("DefaultNegative");

        /// <summary>
        /// Stores download URL, keywords, and selectors of each configured Stable Diffusion Model for VoidMoA.
        /// </summary>
        public static readonly ArtistConfiguration[] artistConfigs = ParseArtists();

        /// <summary>
        /// Convert a download <paramref name="url"/> to a Pickle Tensor filename by stripping URL parameters and adding the extension.
        /// </summary>
        public static string GetPTFilename(string url) => GetSafetensorFilenameWithoutExtension(url) + ".pt";

        /// <summary>
        /// Convert a download <paramref name="url"/> to a safetensor filename by stripping URL parameters and adding the extension.
        /// </summary>
        public static string GetSafetensorFilename(string url) => GetSafetensorFilenameWithoutExtension(url) + ".safetensors";

        /// <summary>
        /// Convert a download <paramref name="url"/> to a safetensor filename root by stripping URL parameters.
        /// </summary>
        public static string GetSafetensorFilenameWithoutExtension(string url) {
            int paramsFrom = url.IndexOf('?');
            return Path.GetFileName(paramsFrom == -1 ? url : url[..paramsFrom]);
        }

        /// <summary>
        /// Parse keywords or selectors to a binary searchable (sorted) array.
        /// </summary>
        internal static string[] ReadKeywordList(string key) {
            string[] result = ReadList(key);
            for (int i = 0; i < result.Length; i++) {
                result[i] = result[i].ToLowerInvariant();
            }
            Array.Sort(result);
            return result;
        }

        /// <summary>
        /// Get every custom artist related configuration in a structured format.
        /// </summary>
        static ArtistConfiguration[] ParseArtists() {
            List<ArtistConfiguration> artists = [];
            foreach (string artist in ForEach("Artist")) {
                artists.Add(new(artist));
            }
            return [.. artists];
        }

        /// <summary>
        /// Get the X component of a resolution variable of WxH format.
        /// </summary>
        static int ResX(string key) {
            string value = Values[key];
            return int.Parse(value[..value.IndexOf('x')]);
        }

        /// <summary>
        /// Get the Y component of a resolution variable of WxH format.
        /// </summary>
        static int ResY(string key) {
            string value = Values[key];
            return int.Parse(value[(value.IndexOf('x') + 1)..]);
        }
    }
}