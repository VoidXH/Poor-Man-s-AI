using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

using PoorMansAI.Configuration;

namespace PoorMansAI.Extensions {
    /// <summary>
    /// Adds new behavior to extension points.
    /// </summary>
    public abstract class Extension {
        /// <summary>
        /// Format or otherwise transform generated chat messages.
        /// </summary>
        /// <param name="history">Previous chat messages in the conversation</param>
        /// <param name="output">The next chat message written to the user</param>
        public delegate void ChatMessageDelegate(JsonArray history, ref string output);

        /// <summary>
        /// Called every 10 seconds while the application is running.
        /// </summary>
        public static event Action PeriodicActions;

        /// <summary>
        /// Called after each chat message.
        /// </summary>
        public static event ChatMessageDelegate ChatPostprocessActions;

        /// <summary>
        /// Register the extensions enabled in the <see cref="Config"/>.
        /// </summary>
        public static void RegisterAll() {
            string[] extensions = Config.extensions;
            for (int i = 0; i < extensions.Length; i++) {
                switch (extensions[i]) {
                    case nameof(ChatPostprocessor):
                        new ChatPostprocessor().Register();
                        break;
                    case nameof(LocalIPLogger):
                        new LocalIPLogger().Register();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Register this extension to the corresponding event.
        /// </summary>
        protected internal abstract void Register();

        /// <summary>
        /// Called from the main thread, performs all <see cref="PeriodicActions"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RunPeriodicActions() => PeriodicActions?.Invoke();

        /// <summary>
        /// Called from the main thread, performs all <see cref="PeriodicActions"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RunChatPostprocessActions(JsonArray history, ref string output) => ChatPostprocessActions?.Invoke(history, ref output);
    }
}