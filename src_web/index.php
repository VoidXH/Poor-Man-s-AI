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
$moaClass = $mode == 5 ? "text-primary" : "text-secondary";
$llmClass = $mode == 2 ? "text-primary" : "text-secondary";

function online() {
  global $online; ?>
<span class="input-group-text bg-<?=$online ? "success" : "danger" ?> text-white"><?=$online ? "On" : "Off" ?>line</span>
<?php } ?>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title><?=$siteName ?></title>
  <link rel="stylesheet" href="<?=$bootstrapPath ?>">
  <link rel="stylesheet" href="css/index.css">
  <link rel="stylesheet" href="css/dark.css">
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
  <a class="br" href="https://github.com/VoidXH/Poor-Man-s-AI"><img src="img/github.svg"></a>
  <div class="server input-group mt-3">
    <?php if ($admin) { ?>
    <div class="input-group-prepend">
      <?php online(); ?>
    </div>
    <div class="input-group-append">
<?php
require("proc/addon.php");
addon("adminbtn");
?>
      <a class="btn btn-secondary" href="config.php">Config</a>
      <?php if ($online) { ?>
      <div class="btn btn-primary m-0 p-0 dropdown show">
        <a class="btn btn-primary dropdown-toggle" href="#" role="button" id="modeList" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">Mode</a>
        <div class="dropdown-menu" aria-labelledby="modeList">
          <a class="<?=$moaClass ?> dropdown-item" onclick="enableMoA()">Image + SLM</a>
          <a class="<?=$llmClass ?> dropdown-item" onclick="enableLLM()">LLM</a>
        </div>
      </div>
      <?php } ?>
    </div>
    <?php } else if ($open && !$uid) { ?>
    <div class="input-group-prepend">
      <?php online(); ?>
    </div>
    <div class="input-group-append">
      <a class="btn btn-primary" href="login.php">Login</a>
    </div>
    <?php } else {
      online();
    } ?>
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
  <?php if ($admin) { ?>
  <script src="<?=$popperPath ?>"></script>
  <script src="<?=$bootstrapJSPath ?>"></script>
  <?php } ?>
  <script src="js/command.js"></script>
  <script src="js/index.js"></script>
</body>
</html>
