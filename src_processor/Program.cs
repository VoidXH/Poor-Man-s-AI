using PoorMansAI;
using PoorMansAI.Configuration;

Downloader.Prepare(); // Download required models

// Process commands until closed
CommandRunner runner = new();
Console.CancelKeyPress += (_, e) => {
    runner.Dispose();
    runner = null;
};
while (runner != null) {
    Thread.Sleep(10000);
}