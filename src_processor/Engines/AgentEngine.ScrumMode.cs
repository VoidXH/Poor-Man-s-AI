using System.Text;

using PoorMansAI.Engines.BaseClasses;

namespace PoorMansAI.Engines {
    // A mode in the Agent engine that splits the tasks and works on each until it's done
    partial class AgentEngine {
        string RunInScrumMode(Command command) {
            StringBuilder output = new();
            object tasks = SplitTask(command, command.Prompt, output);
            if (promptCancellers[command.ID].IsCancellationRequested) {
                return null;
            }

            PerformTasks(command, tasks, output);
            return CutOutput(output.ToString());
        }

        /// <summary>
        /// Splits the task to subtasks. Each returned instance is either a subtask (string) or a set of further subtasks (object[]), recursively.
        /// </summary>
        object SplitTask(Command command, string task, StringBuilder output) {
            if (promptCancellers[command.ID].IsCancellationRequested) {
                return null;
            }

            Engine chat = Others.TryGetValue(EngineType.Chat, out Engine chatInstance) ? chatInstance : this; // Perform on an Agent if Chat is not running locally
            string[] split = chat.Generate(new(EngineType.Chat, string.Format(splitterPrompt, task))).Split('\n', StringSplitOptions.RemoveEmptyEntries);
            string[] tasks = [.. split.Where(x => x.StartsWith('-')).Select(x => x[1..].Trim())];
            if (tasks.Length == 0) {
                return task;
            }

            object[] result = new object[tasks.Length];
            output.Append("<ul>");
            for (int i = 0; i < tasks.Length; i++) {
                output.Append("<li>");
                output.Append(tasks[i]);
                object subtasks = SplitTask(command, tasks[i], output);
                if (subtasks is string) {
                    result[i] = tasks[i];
                } else {
                    result[i] = subtasks;
                }
                output.Append("</li>");
                UpdateProgress(command, .5f, CutOutput(output.ToString()));
            }
            output.Append("</ul>");
            return result;
        }

        /// <summary>
        /// Recursively perform the tasks in the previously created plan.
        /// </summary>
        void PerformTasks(Command command, object tasks, StringBuilder output) {
            if (tasks is string task) {
                output.Append("Performing the following task: <i>").Append(task).Append("</i><br>");
                UpdateProgress(command, .5f, CutOutput(output.ToString()));
                output.Append(Generate(new(EngineType.Agent, task))); // TODO: retry until not done
                UpdateProgress(command, .5f, CutOutput(output.ToString()));
            } else if (tasks is object[] subtasks) {
                foreach (object subtask in subtasks) {
                    PerformTasks(command, subtask, output);
                }
            }
        }

        const string splitterPrompt = "Break this task to smaller tasks to give them to a developer or tell me if it's enough as one task entry: \"{0}\" " +
            "If it should be broken into smaller tasks, use a bullet point list. Do not answer anything else but a list of bullet points. If the task is enough for " +
            $"one developer, say \"{end}\" When you use bullet points, make sure they don't need to know what the root task or any other task is, " +
            "so they need to refer to the already existing code parts and include how they are relevant to the main task - if the main task is needed for context, " +
            "include that too.";

        const string end = "Task shouldn't be divided.";
    }
}
