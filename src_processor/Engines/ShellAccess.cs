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

        /// <summary>
        /// The currently executing command.
        /// </summary>
        Command currentCommand;

        /// <summary>
        /// Provides remote access to the system shell.
        /// </summary>
        public ShellAccess() => session.OnOutputReceived += HandleOutput;

        /// <inheritdoc/>
        protected override void Process(Command command) {
            currentCommand = command;
            session.SendInput(command.Prompt);
        }

        /// <inheritdoc/>
        public override void Dispose() {
            session.OnOutputReceived -= HandleOutput;
            GC.SuppressFinalize(this);
            session.Dispose();
        }

        /// <summary>
        /// Called when the shell session produces output.
        /// </summary>
        void HandleOutput(string data) {
            if (currentCommand != null) {
                UpdateProgress(currentCommand, 1f, data);
            }
        }
    }
}
