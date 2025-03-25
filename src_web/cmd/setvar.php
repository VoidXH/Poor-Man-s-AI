<?php
require("../_check.php");
require("../proc/ai_vars.php");

if (!$admin) {
    die;
}

$key = $_POST["key"];
$stmt->execute("SELECT 1 FROM ai_vars WHERE key_column = ? LIMIT 1", $key);
$exists = $stmt->get_result()->num_rows != 0;
$stmt->close();

if ($exists) {
    setAIVar($key, $_POST["value"]);
} else {
    $stmt = execute("INSERT INTO ai_vars (`key`, `value`) VALUES (?, ?)", $key, $_POST["value"]);
    $stmt->close();
}
?>