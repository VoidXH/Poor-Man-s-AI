<?php
require_once("../_check.php");
if (!admin) {
    die;
}

$stmt = execute("SELECT data FROM ai_reports WHERE time = ?", $_GET["id"]);
$stmt->bind_result($data);
$stmt->fetch();
$stmt->close();
?>
<html lang="en" class="theme-<?=$theme ?>">
<head>
    <meta charset="UTF-8">
    <?=$viewport ?>
    <title>Dislike Dump</title>
    <link rel="stylesheet" href="<?=$bootstrapPath ?>">
    <link rel="stylesheet" href="../css/dark.css">
</head>
<body>
<div class="container">
    <div class="card">
        <div class="card-header">
            <span class="text-center flex-grow-1">Disliked Conversation Dump</span>
        </div>
        <div class="card-body">
            <pre style="white-space: pre-wrap; word-break: break-word;"><?=htmlspecialchars($data) ?></pre>
        </div>
    </div>
</div>
<script src="<?=$jqueryPath ?>"></script>
<script src="<?=$bootstrapJSPath ?>"></script>
<a class="br" href="https://github.com/VoidXH/Poor-Man-s-AI"><img src="../img/github.svg"></a>
</body>
</html>