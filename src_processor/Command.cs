using System.Text.Json.Nodes;

using PoorMansAI.Engines;

namespace PoorMansAI {
    /// <summary>
    /// User-entered prompt paired to an <see cref="Engine"/>.
    /// </summary>
    public class Command {
        /// <summary>
        /// The <see cref="Engine"/> running this <see cref="Command"/>.
        /// </summary>
        public EngineType EngineType { get; }

        /// <summary>
        /// ID of the user query to assign the result to.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// What the user wants.
        /// </summary>
        public string Prompt { get; }

        /// <summary>
        /// Used for making sure we don't waste bandwidth for reporting the same exact result as before.
        /// </summary>
        string lastStatus;

        /// <summary>
        /// User-entered prompt paired to an <see cref="Engine"/>.
        /// </summary>
        /// <param name="entry">Server-sent command entry.</param>
        public Command(JsonNode entry) {
            ID = int.Parse(entry["id"].GetValue<string>());
            string command = entry["command"].GetValue<string>();
            int split = command.IndexOf('|');
            EngineType = Enum.Parse<EngineType>(command[..split]);
            Prompt = command[(split + 1)..];
        }

        /// <summary>
        /// Check if this status was already sent to the server. If yes, it will be nullified to prevent redundantly used bandwidth,
        /// unless giving a result is <paramref name="forced"/>.
        /// </summary>
        /// <returns>What needs to be sent to the server.</returns>
        public string Update(string status, bool forced) {
            if (lastStatus == status && !forced) {
                return null;
            }
            lastStatus = status;
            return status;
        }
    }
}