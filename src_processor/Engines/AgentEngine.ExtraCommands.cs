using System.Collections.Concurrent;
using System.Text;

using PoorMansAI.Data;

namespace PoorMansAI.Engines;

partial class AgentEngine {
    /// <summary>
    /// Queued agent tasks.
    /// </summary>
    readonly ConcurrentQueue<string> agentQueue = [];

    /// <summary>
    /// Stops execution of the <see cref="queueRunner"/>.
    /// </summary>
    CancellationTokenSource queueCanceller;

    /// <summary>
    /// Processes the queue when no other tasks are in progress.
    /// </summary>
    Thread queueRunner;

    /// <summary>
    /// The first task of the <see cref="agentQueue"/> is being processed.
    /// </summary>
    bool queuedProcessed;

    /// <summary>
    /// Formats a queue item from <c>path|prompt</c> to <c>&lt;b&gt;last_folder_name:&lt;/b&gt; prompt</c>.
    /// </summary>
    static string FormatQueueItem(string item) {
        int split = item.LastIndexOf('|');
        if (split <= 0) {
            return item;
        }

        string path = item[..split];
        string prompt = item[(split + 1)..];
        string lastFolder = Path.GetFileName(path);
        return $"<b>{lastFolder}:</b> {prompt}";
    }

    /// <summary>
    /// Reads a file's contents and returns them as pre-formatted text.
    /// </summary>
    static string ReadFile(string path) {
        if (!File.Exists(path)) {
            return string.Format(fileNotFound, path);
        }

        try {
            string language = Markdown.GetLanguageCodeword(Path.GetExtension(path));
            string content = File.ReadAllText(path);
            return $"## Contents of {path}\n```{language}\n{content}\n```\n------------------------------";
        } catch (Exception ex) {
            return $"<p style=\"color:red;\">Error reading file: {path} - {ex.Message}</p>";
        }
    }

    /// <summary>
    /// Handler for extra commands enclosed in square brackets at the start of a prompt.
    /// If the command is valid, it's removed from the prompt and its result is appended to the result.
    /// </summary>
    string ExtraCommandHandler(string projectFolder, string command, int commandID) => command switch {
        string cmd when cmd.StartsWith("File:") => cmd[5..].StartsWith(projectFolder) ? ReadFile(cmd[5..]) : string.Format(fileNotFound, cmd[5..]),
        string task when task.StartsWith("Queue:") => AddToQueue(task[6..]),
        string task when task.StartsWith("Scrum:") => RunInScrumMode(new(EngineType.Agent, commandID, task)),
        "Files" => FileSystem.GetTree(projectFolder, ".git", ".vs", ".vscode"),
        "GitDiff" => ChangedFiles.GetAllDiffs(projectFolder),
        "GitStatus" => string.Join("<br>", ChangedFiles.GetChangedFiles(projectFolder)),
        "Queue" => ShowQueue(),
        "QueueClear" => ClearQueue(),
        _ => null
    };

    /// <summary>
    /// Queue an agentic task for evaluation when queue processing is started.
    /// </summary>
    string AddToQueue(string task) {
        agentQueue.Enqueue(task);
        return "Task queued: " + task;
    }

    /// <summary>
    /// Remove all queued tasks except the first one (which is currently being processed).
    /// </summary>
    string ClearQueue() {
        if (agentQueue.TryDequeue(out string first)) {
            agentQueue.Clear();
            agentQueue.Enqueue(first);
        }
        return "Queue cleared.";
    }

    /// <summary>
    /// List the currently queued tasks.
    /// </summary>
    string ShowQueue() {
        if (agentQueue.IsEmpty) {
            return "Queue is empty.";
        }

        StringBuilder result = new("<ul>");
        bool showHandled = queuedProcessed;
        foreach (string item in agentQueue) {
            string display = FormatQueueItem(item);
            result.Append("<li>").Append(display);
            if (showHandled) {
                result.Append(" <b>(in progress)</b>");
                showHandled = false;
            }
            result.Append("</li>");
        }
        return result.Append("</ul>").ToString();
    }

    /// <summary>
    /// Starts background processing of queued agentic tasks.
    /// </summary>
    void LaunchQueueThread() {
        queueCanceller = new();
        queueRunner = new(QueueRunner);
        queueRunner.Start();
    }

    /// <summary>
    /// A thread processing queued agentic tasks in the background.
    /// </summary>
    void QueueRunner() {
        while (!queueCanceller.IsCancellationRequested) {
            Thread.Sleep(1000);
            if (agentQueue.TryPeek(out string task)) {
                queuedProcessed = true;
                Generate(new(EngineType.Agent, task));
                if (agentQueue.TryPeek(out string peek)) {
                    if (task == peek) { // Not thread-safe, but highly unlikely to break here and not fatal
                        agentQueue.TryDequeue(out _);
                    }
                }
                queuedProcessed = false;
            }
        }
    }

    /// <summary>
    /// Stops background processing of queued agentic tasks.
    /// </summary>
    void StopQueueThread() {
        queueCanceller.Cancel();
        queueRunner.Join();
        queueCanceller = null;
    }

    /// <summary>
    /// Error message for wrong paths.
    /// </summary>
    const string fileNotFound = "<p style=\"color:red;\">File not found: {0}</p>";
}
