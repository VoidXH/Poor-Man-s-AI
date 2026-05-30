<?php
/*
	Create job endpoint
	-------------------
	POST: command=<command string>
*/

require(__DIR__ . '/../../_check.php');
require(__DIR__ . '/../../proc/addon.php');
require(__DIR__ . '/../../proc/ai_vars.php');

function getQueueLength() {
    global $sqlink, $commandTS;
    $result = $sqlink->query("SELECT COUNT(*) FROM ai_commands WHERE command_ts < '$commandTS' AND progress != 100");
    $row = $result->fetch_assoc();
    return $row['COUNT(*)'];
}

if (getQueueLength() >= $maxQueueLength) {
    http_response_code(503);
    die;
}

$command = $_POST['command'];
$pipePos = strpos($command, '|');
$isShell = $pipePos === false || strpos(substr($command, 0, $pipePos), 'Shell') !== false;
if ($isShell && !$admin) {
    die('100|');
}

addon('command_before');

$commandTS = $admin ? '\''.date('Y-m-d H:i:s', strtotime((1 - $commandClear).' minutes')).'\'' : 'NOW()';
$stmt = execute("INSERT INTO ai_commands (command, command_ts, progress) VALUES (?, $commandTS, 0)", $command);
echo $stmt->insert_id;
$stmt->close();

if ($uid) {
    $stmt = execute('UPDATE ai_users SET prompts = prompts + 1 WHERE id = ?', $uid);
    $stmt->close();
} else {
    setAIVar('unreg-use', intval(getAIVar('unreg-use')) + 1);
}

addon('command_after');
die;
?>
