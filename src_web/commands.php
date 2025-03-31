<?php
/*
  User command handling endpoints
  -------------------------------
  GET:
    ?check=ID: get the progress and status message for a job with an ID, separated by |
  POST:
    command: creates a job and prints its ID
    stop: stops a job by ID
*/

require_once("_check.php");

$method = $_SERVER["REQUEST_METHOD"];
if ($method === "GET") {
    if (isset($_GET["check"])) {
        require("proc/addon.php");
        addon("check");
        $id = $_GET["check"];
        $stmt = execute("SELECT command_ts, progress, result FROM ai_commands WHERE id = ?", $id);
        $stmt->bind_result($command_ts, $progress, $result);
        $stmt->fetch();
        $stmt->close();
        if ($command_ts == 0) {
            die("100|");
        }
        if ($progress == 0) {
            $result = $sqlink->query("SELECT COUNT(*) FROM ai_commands WHERE command_ts < '$command_ts' AND progress != 100");
            $row = $result->fetch_assoc();
            die((-1 - $row["COUNT(*)"]) . "|");
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
            require_once("proc/ai_vars.php");
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