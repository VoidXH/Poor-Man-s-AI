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
    /// Reads a file's contents and returns them as pre-formatted text.
    /// </summary>
    static string ReadFile(string path) {
        if (!File.Exists(path)) {
            return $"<p style=\"color:red;\">File not found: {path}</p>";
        }

        try {
            string content = File.ReadAllText(path).Replace("<", "&lt;").Replace(">", "&gt;");
            return $"<pre>{content}</pre>";
        } catch (Exception ex) {
            return $"<p style=\"color:red;\">Error reading file: {path} - {ex.Message}</p>";
        }
    }

    /// <summary>
    /// Handler for extra commands enclosed in square brackets at the start of a prompt.
    /// If the command is valid, it's removed from the prompt and its result is appended to the result.
    /// </summary>
    string ExtraCommandHandler(string projectFolder, string command) => command switch {
        string cmd when cmd.StartsWith("File:") => ReadFile(cmd[5..]),
        string task when task.StartsWith("Queue:") => AddToQueue(task[6..]),
        "Files" => FileSystem.GetTree(projectFolder, ".git", ".vs", "bin", "obj", "Library"),
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
    /// Remove all queued tasks.
    /// </summary>
    string ClearQueue() {
        agentQueue.Clear();
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
            result.Append("<li>").Append(item);
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
                agentQueue.TryDequeue(out _);
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
}
