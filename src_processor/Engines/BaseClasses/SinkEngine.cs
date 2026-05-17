namespace PoorMansAI.Engines.BaseClasses {
    /// <summary>
    /// Engines that don't reply to single commands, but are a constantly running singleton.
    /// </summary>
    public abstract class SinkEngine<T> : Engine where T : SinkEngine<T>, new() {
        /// <summary>
        /// The currently running instance of the engine.
        /// </summary>
        public static SinkEngine<T> Instance {
            get {
                instance ??= new T();
                return instance;
            }
        }
        static SinkEngine<T> instance;

        /// <summary>
        /// Multiple instantiation protection.
        /// </summary>
        protected SinkEngine() {
            if (instance != null) {
                throw new InvalidOperationException("Only use the Instance accessor.");
            }
        }

        /// <summary>
        /// An operation to perform on the running engine.
        /// </summary>
        protected abstract void Process(Command command);

        /// <inheritdoc/>
        public sealed override string Generate(Command command) {
            Process(command);
            return string.Empty;
        }

        /// <inheritdoc/>
        public sealed override void StopGeneration() => throw new InvalidOperationException();
    }
}
