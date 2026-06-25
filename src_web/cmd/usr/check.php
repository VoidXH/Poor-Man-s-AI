<?php
/*
	Check job status endpoint
	--------------------------
	GET: ?id=ID — get the progress and status message for a job
*/

require(__DIR__ . '/../../_check.php');
require(__DIR__ . '/../../proc/addon.php');

addon('check');

$id = $_GET['check'];
$stmt = execute('SELECT command, command_ts, progress, result FROM ai_commands WHERE id = ?', $id);
$stmt->bind_result($command, $commandTS, $progress, $result);
$stmt->fetch();
$stmt->close();

if ($commandTS == 0) {
	die('100|');
}

$pipePos = strpos($command, '|');
$commandType = $pipePos === false ? $command : substr($command, 0, $pipePos);
$isAgent = $commandType === 'Agent';
if ($isAgent && !$admin) {
	die('100|');
}

if ($isAgent) {
	$stmt = execute("UPDATE ai_commands SET command_ts = NOW() WHERE id = ?", $id);
	$stmt->close();
}

if ($progress == 0) {
	$result = $sqlink->query("SELECT COUNT(*) FROM ai_commands WHERE command_ts < '$commandTS' AND progress != 100");
	$row = $result->fetch_assoc();
	die((-1 - $row['COUNT(*)']) . '|');
}

header('Content-Type: text/plain; charset=utf-8');
echo $progress . '|' . $result;

if ($progress == 100) {
	$stmt = execute('DELETE FROM ai_commands WHERE id = ?', $id);
} else if ($result != null) {
	$stmt = execute('UPDATE ai_commands SET result = NULL WHERE id = ?', $id);
}
$stmt->close();
die;
?>
