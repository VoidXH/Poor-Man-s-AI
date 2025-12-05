using PoorMansAI.Configuration;

namespace PoorMansAI.NewTech.VoidMoA; 

/// <summary>
/// Assigns a model based on the present keywords. Super fast and somewhat accurate, but can't account for special cases like languages.
/// </summary>
public class KeywordBasedPromptTransformer : PromptTransformer {
    /// <inheritdoc/>
    protected override ArtistConfiguration AssignModel(string prompt, string[] keywords, ref string[] selectors) {
        ArtistConfiguration[] artists = Config.artistConfigs;
        int[] votes = new int[artists.Length];
        int maxVotes = 0,
            maxVotesFor = 0;
        for (int i = 0; i < artists.Length; i++) {
            votes[i] = HasSelector(ref selectors, artists[i].selectors) + HasKeyword(keywords, artists[i].keywords);
            if (maxVotes < votes[i]) {
                maxVotes = votes[i];
                maxVotesFor = i;
            }
        }

        if (maxVotes == 0) {
            return null;
        }
        return artists[maxVotesFor];
    }
}
