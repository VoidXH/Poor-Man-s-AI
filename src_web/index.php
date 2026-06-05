<?php
require_once("proc/loading.php");

$time = time();
$online = $time - getAIVar("llm-available") <= 10 || $time - getAIVar("moa-available") <= 10;
$mode = getAIVar("mode");
$moaClass = $mode == 5 ? "text-primary" : "text-secondary";
$llmClass = $mode == 2 ? "text-primary" : "text-secondary";
?>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title><?=$siteName ?></title>
  <link rel="stylesheet" href="<?=$bootstrapPath ?>">
<?php if (!$instantMode) { ?>
  <link rel="stylesheet" href="css/index.css">
<?php } ?>
  <link rel="stylesheet" href="css/dark.css">
</head>
<?php if ($instantMode) { ?>
<body>
<?php
  require("chat.php"); 
} else {
?>
<body style="display: flex">
<?php 
  require("index_selector.php");
}
?>
  <div id="loading-overlay" class="overlay">
    <div class="spinner"></div>
  </div>
  <a class="br" href="https://github.com/VoidXH/Poor-Man-s-AI"><img src="img/github.svg"></a>
<?php if ($instantMode) { ?>
<script src="<?=$jqueryPath ?>"></script>
<?php
} else {
    require("proc/status.php");
    require("proc/profile.php");
?>
<script src="js/index.js"></script>
<?php } ?>
  <script src="<?=$bootstrapJSPath ?>"></script>
  <script src="js/command.js"></script>
</body>
</html>
