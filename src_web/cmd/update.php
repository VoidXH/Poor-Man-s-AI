<?php
/*
	Progress update endpoint
	------------------------
	POST:
		id: job ID
		result: new status text (or base64 binary)
		progress: new progress (1-100)
*/

require_once("availability.php");

if ($admin && isset($_POST["id"])) {
	$id = intval($_POST["id"]);
	$newProgress = intval($_POST["progress"]);
	$result = $_POST["result"];
	$stmt = execute("SELECT command, progress FROM ai_commands WHERE id = ?", $id);
	$stmt->bind_result($command, $progress);
	$rowExists = $stmt->fetch();
	$stmt->close();

	if (!$rowExists) {
		echo "STOP";
		die;
    }

	if ($newProgress == 100) {
		if ($result == null) {
			echo "RETRY Final result is null.";
			die;
		} else if (empty($result)) {
			echo "RETRY Final result is empty.";
			die;
		}
	} else if ($progress == -1) {
		echo "STOP";
		die;
	}

	$pipePos = strpos($command, '|');
	$isAgent = $pipePos === false || strpos(substr($command, 0, $pipePos), 'Agent') !== false;
	$timestampUpdate = $isAgent && $newProgress != 100 ? "command_ts = NOW(), " : "";
	$stmt = execute("UPDATE ai_commands SET result = ?, progress = ?, $timestampUpdate result_ts = NOW() WHERE id = ?", $result, $newProgress, $id);
	$stmt->close();
}
?>
