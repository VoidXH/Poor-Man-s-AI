<?php
require_once("_check.php");
if (!$admin) {
    die;
}

$username = $_COOKIE['username'];
?>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <?=$viewport ?>
    <title><?=$siteName ?> admin - Shell</title>
    <link rel="stylesheet" href="<?=$bootstrapPath ?>">
    <link rel="stylesheet" href="css/dark.css">
    <link rel="stylesheet" href="css/shell.css">
</head>
<body>
<div class="container">
    <div class="card">
        <div class="card-header d-flex align-items-center">
            <a class="btn btn-primary" href="?p=1">Back</a>
            <span class="text-center flex-grow-1">Terminal - <?=$siteName ?></span>
            <div class="w-auto"></div>
        </div>
        <div class="console-card">
            <div class="console-toolbar">
                <span class="toolbar-dot red"></span>
                <span class="toolbar-dot yellow"></span>
                <span class="toolbar-dot green"></span>
                <span class="toolbar-title">shell@<?=$siteName ?>:~</span>
            </div>
            <div class="console-body">
                <div class="output-block">
                    <pre id="output"></pre>
                </div>
            </div>
            <div class="console-footer">
                <div class="input-group">
                    <span class="input-group-text prompt-path"><span class="prompt-user"><?=$username ?>@<?=$siteName ?></span>:<span class="prompt-dir">~</span>$</span>
                    <input type="text" class="form-control prompt-input-field" id="cmd" placeholder="Enter shell command..." autofocus>
                    <button class="btn btn-primary" id="sendBtn">Send</button>
                </div>
            </div>
        </div>
    </div>
</div>
<a class="br" href="https://github.com/VoidXH/Poor-Man-s-AI"><img src="img/github.svg"></a>
<script>
const promptUser = "<?=htmlspecialchars($username) ?>";
const siteName = "<?=htmlspecialchars($siteName) ?>";
</script>
<script src="<?=$jqueryPath ?>"></script>
<script src="<?=$bootstrapJSPath ?>"></script>
<script src="js/command.js"></script>
<script src="js/shell.js"></script>
</body>
</html>
