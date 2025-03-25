using System.Diagnostics;

namespace PoorMansAI.Engines {
    /// <summary>
    /// Keep a process running, even after crashes, until disposed.
    /// </summary>
    class Watchdog : IDisposable {
        /// <summary>
        /// Periodically checks if the process is running, and if not, restarts it.
        /// </summary>
        readonly Thread watcher;

        /// <summary>
        /// Process handle to check the status.
        /// </summary>
        Process instance;

        /// <summary>
        /// Operation that launches a new <see cref="instance"/>.
        /// </summary>
        Func<Process> launcher;

        /// <summary>
        /// First startup successful.
        /// </summary>
        bool started;

        /// <summary>
        /// Keep a process running, even after crashes, until disposed.
        /// </summary>
        /// <param name="launcher">Operation that launches a new <see cref="instance"/></param>
        public Watchdog(Func<Process> launcher) {
            this.launcher = launcher;
            watcher = new Thread(new ThreadStart(WatcherThread));
            watcher.Start();

            // Block until first launch
            while (!started) {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Periodically checks if the process is running, and if not, restarts it.
        /// </summary>
        public void WatcherThread() {
            while (launcher != null) {
                if (instance == null || instance.HasExited) {
                    try {
                        instance = launcher();
                    } catch {
                        continue;
                    }
                    started = true;
                }
                Thread.Sleep(100);
            }
        }

        /// <inheritdoc/>
        public void Dispose() {
            launcher = null;
            watcher?.Join();
            instance?.Kill(true);
        }
    }
}