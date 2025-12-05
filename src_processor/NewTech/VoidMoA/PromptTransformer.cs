using PoorMansAI.Configuration;

namespace PoorMansAI.NewTech.VoidMoA;

/// <summary>
/// Extracts generation settings from a general prompt.
/// </summary>
public abstract class PromptTransformer : IDisposable {
    /// <summary>
    /// Extracts generation settings from a general <paramref name="prompt"/>.
    /// </summary>
    public TransformedPrompt Transform(string prompt) {
        TransformedPrompt result = new();
        const string base64Header = "data:image/png;base64,";
        int pipe = prompt.IndexOf('|');
        if (prompt.StartsWith(base64Header) && pipe != -1) {
            result.Endpoint = "img2img";
            result.ReferenceImages = [prompt[base64Header.Length..pipe]];
            prompt = prompt[(pipe + 1)..];
        } else {
            result.Endpoint = "txt2img";
        }

        string[] keywords = SplitKeywords(prompt); // Every single word
        string[] selectors = SplitSelectors(prompt); // Potential resolution or other property selectors
        ArtistConfiguration artist = AssignModel(prompt, keywords, ref selectors);
        if (artist != null) {
            result.Model = Config.GetSafetensorFilenameWithoutExtension(artist.url);
            result.NegativePrompt = artist.negative;
            result.Guidance = artist.guidance;
            result.Sampler = artist.sampler;
        } else {
            result.Model = Config.GetSafetensorFilenameWithoutExtension(Config.defaultArtist);
            result.NegativePrompt = Config.defaultNegative;
            result.Guidance = Config.imageGenGuidance;
            result.Sampler = Config.imageGenSampler;
        }

        SetupResolution(result, keywords, ref selectors);
        result.Prompt = string.Join(", ", selectors);
        return result;
    }

    /// <summary>
    /// Get the keywords as separate array entries.
    /// </summary>
    protected static string[] SplitKeywords(string prompt) =>
        [.. prompt.Split(keywordSplits, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLowerInvariant())];

    /// <summary>
    /// Get the selectors as separate array entries.
    /// </summary>
    protected static string[] SplitSelectors(string prompt) =>
        [.. prompt.Split(selectorSplits, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLowerInvariant())];

    /// <summary>
    /// Check if a keyword for some action is present in the prompt. Will not remove the keyword from the prompt.
    /// </summary>
    /// <returns>Number of matching keywords.</returns>
    protected static int HasKeyword(string[] promptKeywords, string[] searchedKeywords) {
        int count = 0;
        for (int i = 0; i < promptKeywords.Length; i++) {
            if (Array.BinarySearch(searchedKeywords, promptKeywords[i]) >= 0) {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Check if a selector is defined and remove it from the prompt.
    /// </summary>
    /// <returns>Number of matching selectors.</returns>
    protected static int HasSelector(ref string[] promptSelectors, string[] searchedSelectors) {
        int count = 0;
        for (int i = 0; i < promptSelectors.Length; i++) {
            if (Array.BinarySearch(searchedSelectors, promptSelectors[i]) >= 0) {
                for (int j = i + 1; j < promptSelectors.Length; j++) {
                    promptSelectors[j - 1] = promptSelectors[j];
                }
                Array.Resize(ref promptSelectors, promptSelectors.Length - 1);
                i--;
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Set the resolution of an already transformed prompt based on keywords.
    /// </summary>
    static void SetupResolution(TransformedPrompt result, string[] keywords, ref string[] selectors) {
        int widescreen = HasSelector(ref selectors, Config.hSelectors) + HasKeyword(keywords, Config.hKeywords);
        int portrait = HasSelector(ref selectors, Config.vSelectors) + HasKeyword(keywords, Config.vKeywords);
        bool hd = HasSelector(ref selectors, Config.hdSelectors) != 0 || HasKeyword(keywords, Config.hdKeywords) != 0;
        if (widescreen > portrait) { // Horizontal
            result.Width = hd ? Config.imageSizeHDHW : Config.imageSizeHW;
            result.Height = hd ? Config.imageSizeHDHH : Config.imageSizeHH;
        } else if (portrait > widescreen) { // Vertical
            result.Width = hd ? Config.imageSizeHDVW : Config.imageSizeVW;
            result.Height = hd ? Config.imageSizeHDVH : Config.imageSizeVH;
        } else { // Regular
            result.Width = hd ? Config.imageSizeHDW : Config.imageSizeW;
            result.Height = hd ? Config.imageSizeHDH : Config.imageSizeH;
        }
    }

    /// <summary>
    /// Select one from the artists to assign to this <paramref name="prompt"/>. Returns null when the default model shall be used.
    /// </summary>
    protected abstract ArtistConfiguration AssignModel(string prompt, string[] keywords, ref string[] selectors);

    /// <inheritdoc/>
    public virtual void Dispose() => GC.SuppressFinalize(this);

    /// <summary>
    /// Characters that separate keywords.
    /// </summary>
    static readonly char[] keywordSplits = [' ', ',', '.', '!', ';', '/'];

    /// <summary>
    /// Characters that separate selectors.
    /// </summary>
    static readonly char[] selectorSplits = [',', '.', '!', ';', '/'];
}