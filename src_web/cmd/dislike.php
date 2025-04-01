<?php
require_once("../_check.php");

if (!isset($_POST['history'])) {
    die;
}

$history = $_POST['history'];
if (empty($history)) {
    die;
}

$stmt = execute("SELECT COUNT(*) FROM ai_reports");
$stmt->bind_result($rows);
$stmt->fetch();
$stmt->close();
if ($rows >= $maxWrongAnswers) {
    die;
}

$len = strlen($history);
if ($len > 65536) {
    $history = substr($history, $len - 65536, 65536);
}
$stmt = execute("INSERT INTO ai_reports (data) VALUES (?)", $history);
?>