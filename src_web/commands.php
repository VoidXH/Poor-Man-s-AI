<?php
/*
  Command handling endpoints
  --------------------------
  GET:
    ?list: list all unfinished jobs, optionally update submodule online statuses
    ?check=ID: get the progress and status message for a job with an ID, separated by |
  POST:
    command: creates a job and prints its ID
    stop: stops a job by ID
    update: update job progress: "update" -> job ID, "result" -> new status message, "progress" -> new progress message
*/

require("__config.php");
require("_check.php");
require("proc/ai_vars.php");

// Update submodule online statuses
if (isset($_GET["llm"])) {
  setAIVar("llm-available", time());
}
if (isset($_GET["moa"])) {
  setAIVar("moa-available", time());
}

// Delete old unprocessed entries
$time = date("Y-m-d H:i:s", strtotime("-$commandClear minutes"));
$sqlink->query("DELETE FROM ai_commands WHERE command_ts < \"$time\"");

$method = $_SERVER["REQUEST_METHOD"];
if ($method === "GET") {
  if ($admin) {
    if (isset($_GET["list"])) {
      $result = $sqlink->query("SELECT id, command FROM ai_commands WHERE progress != 100 ORDER BY command_ts ASC");
      $commands = [];
      while ($row = mysqli_fetch_assoc($result)) {
        $commands[] = [
          "id" => $row["id"],
          "command" => $row["command"]
        ];
      }
      echo json_encode(["commands" => $commands]);
      die;
    }
  }

  if (isset($_GET["check"])) {
    require("proc/addon.php");
    addon("check");
    $id = $_GET["check"];
    $stmt = execute("SELECT progress, result FROM ai_commands WHERE id = ?", $id);
    $stmt->bind_result($progress, $result);
    $stmt->fetch();
    echo $progress . "|" . $result;
    $stmt->close();

    if ($progress == 100) {
      $stmt = execute("DELETE FROM ai_commands WHERE id = ?", $id);
    } else {
      $stmt = execute("UPDATE ai_commands SET result = NULL WHERE id = ?", $id);
    }
    $stmt->close();
    die;
  }
} else if ($method === "POST") {
  if ($admin) {
    if (isset($_POST["update"])) {
      $id = intval($_POST["update"]);
      $newProgress = intval($_POST["progress"]);
      $stmt = execute("SELECT progress FROM ai_commands WHERE id = ?", $id);
      $stmt->bind_result($progress);
      $stmt->fetch();
      $stmt->close();
      if ($progress == -1 && $newProgress != 100) {
          echo "STOP";
          die;
      }

      $stmt = execute("UPDATE ai_commands SET result = ?, progress = ?, result_ts = NOW() WHERE id = ?", $_POST["result"], $newProgress, $id);
      $stmt->close();
      die;
    }
  }

  if (isset($_POST["command"])) {
    require("proc/addon.php");
    addon("command_before");
    $stmt = execute("INSERT INTO ai_commands (command, command_ts, progress) VALUES (?, NOW(), 0)", $_POST["command"]);
    echo $stmt->insert_id;
    $stmt->close();

    if ($uid) {
      $stmt = execute("UPDATE ai_users SET prompts = prompts + 1 WHERE id = ?", $uid);
      $stmt->close();
      die;
    }
    addon("command_after");
  }

  if (isset($_POST["stop"])) {
      $stmt = execute("UPDATE ai_commands SET progress = -1 WHERE id = ?", $_POST["stop"]);
      $stmt->close();
      die;
  }
}
?>
