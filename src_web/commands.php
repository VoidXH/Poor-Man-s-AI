<?php
/*
  Command handling endpoints
  --------------------------
  GET:
    ?list: list all unfinished jobs, optionally update Processor availability
    ?check=ID: get the progress and status message for a job with an ID, separated by |
  POST:
    command: creates a job and prints its ID
    stop: stops a job by ID
*/

require("__config.php");
require("_check.php");
require("proc/ai_vars.php");

if ($admin) {
  function availability($engine) {
    if (isset($_GET[$engine])) {
      $now = time();
      $lastTime = getAIVar($engine . "-available", $now);
      $newWeight = intval($_GET[$engine]);
      $lastWeight = intval(getAIVar($engine . "-weight"));
      if ($lastWeight <= $newWeight || $lastTime + $procTimeout < $now) {
        setAIVar($engine . "-available", time());
        setAIVar($engine . "-weight", $newWeight);
        return true;
      }
    }
    return false;
  }

  $llm = availability("llm");
  $moa = availability("moa");
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
        $command = $row["command"];
        $idx = strpos($command, '|');
        if ($idx === false) continue;
        $type = substr($command, 0, $idx);
        if (($llm && $type == "Chat") || ($moa && $type == "Image")) {
          $commands[] = [
            "id" => $row["id"],
            "command" => $command
          ];
        }
      }
      echo json_encode(["commands" => $commands]);
      die;
    }
  }

  if (isset($_GET["check"])) {
    require("proc/addon.php");
    addon("check");
    $id = $_GET["check"];
    $stmt = execute("SELECT command_ts, progress, result FROM ai_commands WHERE id = ?", $id);
    $stmt->bind_result($command_ts, $progress, $result);
    $stmt->fetch();
    $stmt->close();
    if ($progress == 0) {
      $result = $sqlink->query("SELECT COUNT(*) FROM ai_commands WHERE command_ts < '$command_ts' AND progress != 100");
      $row = $result->fetch_assoc();
      echo (-1 - $row["COUNT(*)"]) . "|";
      die;
    }
    echo $progress . "|" . $result;

    if ($progress == 100) {
      $stmt = execute("DELETE FROM ai_commands WHERE id = ?", $id);
    } else if ($result != null) {
      $stmt = execute("UPDATE ai_commands SET result = NULL WHERE id = ?", $id);
    }
    $stmt->close();
    die;
  }
} else if ($method === "POST") {
  if (isset($_POST["command"])) {
    require("proc/addon.php");
    addon("command_before");
    $commandTS = $admin ? "'".date("Y-m-d H:i:s", strtotime((1 - $commandClear)." minutes"))."'" : "NOW()";
    $stmt = execute("INSERT INTO ai_commands (command, command_ts, progress) VALUES (?, $commandTS, 0)", $_POST["command"]);
    echo $stmt->insert_id;
    $stmt->close();

    if ($uid) {
      $stmt = execute("UPDATE ai_users SET prompts = prompts + 1 WHERE id = ?", $uid);
      $stmt->close();
    } else {
      setAIVar("unreg-use", intval(getAIVar("unreg-use")) + 1);
    }
    addon("command_after");
    die;
  }

  if (isset($_POST["stop"])) {
      $stmt = execute("UPDATE ai_commands SET progress = -1 WHERE id = ?", $_POST["stop"]);
      $stmt->close();
      die;
  }
}
?>
