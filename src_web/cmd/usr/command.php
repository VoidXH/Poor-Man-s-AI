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
$commandType = $pipePos === false ? $command : substr($command, 0, $pipePos);
$adminOnlyCommand = $commandType === 'Agent' || $commandType === 'Mode';
if ($adminOnlyCommand && !$admin) {
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

if ($usageTracking) {
    $range = 900; // 15 minutes in seconds
    $result = $sqlink->query("SELECT COUNT(*) FROM ai_commands WHERE progress != 100");
    if ($result) {
        $row = $result->fetch_assoc();
        $usage = $row["COUNT(*)"];

        $time = time();
        $time = date("Y-m-d H:i:s", floor($time / $range) * $range);
        $result = $sqlink->query("SELECT * FROM ai_usage WHERE time = '$time'");
        if (mysqli_num_rows($result) > 0) {
            $row = $result->fetch_assoc();
            $oldUsage = $row["usage"];
            if ($usage > $oldUsage) {
                $sqlink->query("UPDATE ai_usage SET time = '$time', `usage` = $usage WHERE time = '$time'");
            }
        } else {
            $sqlink->query("INSERT INTO ai_usage (time, `usage`) VALUES ('$time', $usage)");
        }
    }
}

addon('command_after');
die;
?>
