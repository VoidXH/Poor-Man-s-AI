<?php
/*
	Stop job endpoint
	-----------------
	POST: stop=ID — stop a job by its ID
*/

require(__DIR__ . '/../../_check.php');

$stmt = execute('UPDATE ai_commands SET progress = -1 WHERE id = ?', $_POST['stop']);
$stmt->close();
die;
?>
