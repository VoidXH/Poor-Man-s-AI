using System.Runtime.CompilerServices;

namespace PoorMansAI.Engines {
    /// <summary>
    /// One kind of task-specific AI like image generator or chatbot.
    /// </summary>
    public abstract class Engine : IDisposable {
        /// <summary>
        /// Snapshot of progression.
        /// </summary>
        /// <param name="command">All data about the user's query</param>
        /// <param name="progress">Ratio of completion</param>
        /// <param name="status">Current version of the text/image/etc under generation</param>
        public delegate void Progress(Command command, float progress, string status);

        /// <summary>
        /// Progress is periodically reported for subscribed users.
        /// </summary>
        public event Progress OnProgress;

        /// <summary>
        /// Start the text/image/etc generation with a full prompt. This function blocks until the generation is done.
        /// </summary>
        /// <param name="command">All data about the user's query</param>
        public abstract string Generate(Command command);

        /// <summary>
        /// Stop the generation process of the current instance.
        /// </summary>
        public abstract void StopGeneration();

        /// <summary>
        /// Stop the engine.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Invoke <see cref="OnProgress"/>, send a snapshot of progression.
        /// </summary>
        /// <param name="command">All data about the user's query</param>
        /// <param name="progress">Ratio of completion</param>
        /// <param name="status">Current version of the text/image/etc under generation</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdateProgress(Command command, float progress, string status) {
            if (command.ID == -1) {
                return;
            }
            OnProgress?.Invoke(command, progress, status);
        }
    }
}