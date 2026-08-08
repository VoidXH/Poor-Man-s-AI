using System.Text.Json.Nodes;

namespace PoorMansAI.Extensions {
    /// <summary>
    /// Allows modification of chat request parameters before sending to the endpoint.
    /// </summary>
    public partial class ChatPreprocessor : Extension {
        /// <summary>
        /// Delegate for preprocessing chat requests.
        /// </summary>
        /// <param name="endpoint">The endpoint URL (can be modified)</param>
        /// <param name="apiKey">The API key (can be modified)</param>
        /// <param name="messages">The chat messages array (can be modified)</param>
        /// <param name="parameters">Additional parameters (model, temperature, etc.)</param>
        public delegate void ChatPreprocessDelegate(ref string endpoint, ref string apiKey, JsonArray messages, JsonObject parameters);

        /// <summary>
        /// Actions to perform before sending a chat request.
        /// </summary>
        public static event ChatPreprocessDelegate ChatPreprocessActions;

        /// <summary>
        /// Further actions to perform in derived classes.
        /// </summary>
        protected ChatPreprocessDelegate FurtherActions;

        /// <inheritdoc/>
        protected internal override void Register() => ChatPreprocessActions += FurtherActions;

        /// <summary>
        /// Invokes all registered preprocess actions.
        /// </summary>
        /// <param name="endpoint">The endpoint URL (can be modified)</param>
        /// <param name="apiKey">The API key (can be modified)</param>
        /// <param name="messages">The chat messages array (can be modified)</param>
        /// <param name="parameters">Additional parameters (model, temperature, etc.)</param>
        internal static void RunChatPreprocessActions(ref string endpoint, ref string apiKey, JsonArray messages, JsonObject parameters) =>
            ChatPreprocessActions?.Invoke(ref endpoint, ref apiKey, messages, parameters);
    }
}
