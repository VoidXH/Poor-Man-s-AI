<?php
require_once("../_check.php");
if (!admin) {
    die;
}

$stmt = execute("SELECT data FROM ai_reports WHERE time = ?", $_GET["id"]);
$stmt->bind_result($data);
$stmt->fetch();
$stmt->close();
echo $data;
?>