using VoidX.WPF;

using PoorMansAI;
using PoorMansAI.Configuration;
using PoorMansAI.Extensions;
using PoorMansAI.Engines;

Logger.MinLogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), Config.Values["LogLevel"]);
// Prepare the environment, like downloading required models
Downloader.Prepare();
LlamaCpp.Ready();

// Process commands until closed
CommandRunner runner = new();

// Prepare extensions
WebsiteExtension.cookies = runner.cookies;
Extension.RegisterAll();

Console.CancelKeyPress += (_, e) => {
    runner.Dispose();
    runner = null;
};
while (runner != null) {
    Thread.Sleep(10000);
    Extension.RunPeriodicActions();
}