namespace PoorMansAI.Extensions {
    /// <summary>
    /// Performs formattings or transformations on produced chat messages.
    /// </summary>
    public partial class ChatPostprocessor : Extension {
        /// <summary>
        /// Further actions to perform in derived classes.
        /// </summary>
        protected ChatMessageDelegate FurtherActions;

        /// <inheritdoc/>
        protected internal override void Register() {
            ChatPostprocessActions += FurtherActions;
        }
    }
}