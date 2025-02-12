<?php
require("__config.php");
require("_check.php");
require("proc/ai_vars.php");

$gpt_bgs = glob("img/gptbg*.jpg");
$gpt_bg = $gpt_bgs[array_rand($gpt_bgs)];
$diff_bgs = glob("img/diffbg*.jpg");
$diff_bg = $diff_bgs[array_rand($diff_bgs)];

$time = time();
$online = $time - getAIVar("llm-available") <= 10 || $time - getAIVar("moa-available") <= 10;
$mode = getAIVar("mode");
$moaClass = $mode == 5 ? "btn-primary" : "btn-secondary";
$llmClass = $mode == 2 ? "btn-primary" : "btn-secondary";
?>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title><?=$siteName ?></title>
  <link rel="stylesheet" href="<?=$bootstrapPath ?>">
  <link rel="stylesheet" href="css/index.css">
  <style>
    .gpt { background-image: url('<?=$gpt_bg ?>'); }
    .moa { background-image: url('<?=$diff_bg ?>'); }
  </style>
</head>
<body>
  <div class="main">
    <div class="option gpt" onclick="gpt()"><span><?=$chatPage ?></span></div>
    <div class="option moa" onclick="moa()"><span><?=$moaPage ?></span></div>
  </div>
  <div id="loading-overlay" class="overlay">
    <div class="spinner"></div>
  </div>
  <div class="server input-group mt-3">
    <?php if ($online && $admin) { ?>
    <div class="input-group-prepend">
      <span class="input-group-text bg-success text-white">Online</span>
    </div>
    <div class="input-group-append">
      <button class="btn <?=$moaClass ?>" type="button" onclick="enableMoA()">Image + SLM</button>
      <button class="btn <?=$llmClass ?>" type="button" onclick="enableLLM()">LLM</button>
    </div>
    <?php } else if ($open && !$uid) { ?>
    <div class="input-group-prepend">
      <span class="input-group-text bg-<?=$online ? "success" : "danger" ?> text-white"><?=$online ? "On" : "Off" ?>line</span>
    </div>
    <div class="input-group-append">
      <a class="btn btn-primary" href="login.php">Login</a>
    </div>
    <?php } else { ?>
    <span class="input-group-text bg-<?=$online ? "success" : "danger" ?> text-white"><?=$online ? "On" : "Off" ?>line</span>
    <?php } ?>
  </div>
  <?php if ($uid) { ?>
  <div class="input-group profile m-3">
    <div class="input-group-prepend">
      <a class="btn btn-secondary" href="profile.php"><?=htmlspecialchars($_COOKIE["username"]) ?></a>
    </div>
    <div class="input-group-append">
      <a class="btn btn-secondary" href="logout.php">Logout</a>
    </div>
  </div>
  <?php } ?>
  <script src="<?=$jqueryPath ?>"></script>
  <script src="js/command.js"></script>
  <script src="js/index.js"></script>
</body>
</html>
