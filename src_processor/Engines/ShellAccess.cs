using PoorMansAI.Engines.BaseClasses;

namespace PoorMansAI.Engines {
    /// <summary>
    /// Provides remote access to the system shell.
    /// </summary>
    public class ShellAccess : SinkEngine<ShellAccess> {
        /// <summary>
        /// Background handler for the shell instance.
        /// </summary>
        readonly ShellSession session = new();

        /// <inheritdoc/>
        protected override void Process(Command command) => session.SendInput(command.Prompt);

        /// <inheritdoc/>
        public override void Dispose() {
            GC.SuppressFinalize(this);
            session.Dispose();
        }
    }
}
