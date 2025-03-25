using PoorMansAI.Configuration;

namespace PoorMansAI.Extensions {
    /// <summary>
    /// Adds new behavior to extension points.
    /// </summary>
    public abstract class Extension {
        /// <summary>
        /// Called every 10 seconds while the application is running.
        /// </summary>
        public static event Action PeriodicActions;

        /// <summary>
        /// Register the extensions enabled in the <see cref="Config"/>.
        /// </summary>
        public static void RegisterAll() {
            string[] extensions = Config.extensions;
            for (int i = 0; i < extensions.Length; i++) {
                switch (extensions[i]) {
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
        internal static void RunPeriodicActions() => PeriodicActions?.Invoke();
    }
}