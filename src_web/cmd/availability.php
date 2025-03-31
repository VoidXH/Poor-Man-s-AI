<?php
require_once("../_check.php");
if (!$admin) {
    die;
}

require_once("../proc/ai_vars.php");

function availability($engine) {
    global $procTimeout;
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

// Delete old unprocessed entries
$time = date("Y-m-d H:i:s", strtotime("-$commandClear minutes"));
$sqlink->query("DELETE FROM ai_commands WHERE command_ts < \"$time\"");
?>