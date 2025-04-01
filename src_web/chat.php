<?php
require_once("_check.php");
if ($forceLogin && !$uid) {
    require_once("login.php");
}

require_once("proc/addon.php");
require_once("proc/ai_vars.php");

$time = time();
$offline = $time - getAIVar("llm-available") > $procTimeout;
$slm = $time - getAIVar("moa-available") <= $procTimeout;
?>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title><?=$chatName ?></title>
  <link rel="stylesheet" href="<?=$bootstrapPath ?>">
  <link rel="stylesheet" href="css/dark.css">
  <link rel="stylesheet" href="css/chat.css">
</head>
<body>
<div class="container">
  <div class="card">
    <div class="card-header d-flex align-items-center">
      <div class="input-group w-auto">
        <div class="input-group-prepend">
          <a class="btn btn-primary" href="index.php">Back</a>
        </div>
        <div class="input-group-append">
          <button class="input-group-append btn btn-danger" id="reset" onclick="reset()">Reset</button>
        </div>
      </div>
      <span class="text-center flex-grow-1"><?=$chatName ?></span>
      <div class="input-group w-auto">
        <div class="input-group-prepend">
          <label class="input-group-text">Model</label>
        </div>
        <?php
        $models = preg_split('/\s*,\s*/', $chatModels);
        echo "<div class=\"input-group-append\" id=\"model\">";
          foreach ($models as $index => $model) {
            $class = ($index == 0) ? "btn btn-primary" : "btn btn-secondary";
            $id = strtolower($model);
            echo "<button class=\"$class\" id=\"$id\">$model</button>";
          }
        echo "</div>";
        ?>
      </div>
    </div>
    <div class="card-body chatbox">
<?php if ($offline) { ?>
      <div class="alert alert-danger" role="alert">Shhh! The chatting computer is sleeping and can't work now. Wait until it wakes up!</div>
<?php } else {
  if ($slm && $slmWarning) { ?>
      <div class="alert alert-warning" role="alert">The server is currently running the chat on CPU. The quality and generation speed of answers may be worse.</div>
<?php
  }
  if (isset($_POST["fill"])) {
    require("proc/chat/fill.php");
  } else {
    require("proc/chat/starters.php");
  }
}
?>
    </div>
    <div class="card-footer">
      <?php if (!$offline) { ?>
      <div class="input-group">
        <textarea class="form-control" id="input" placeholder="Ask <?=$chatName ?> anything..." autofocus></textarea>
        <div class="input-group-append">
          <button class="btn btn-danger" id="stop" style="display:none;" onclick="stop()">Stop</button>
          <button class="btn btn-primary" id="send" onclick="send()">Send</button>
        </div>
      </div>
      <?php if ($uid) { ?>
      <p class="text-center mt-3"><small><?=$chatName ?> can make mistakes. Check important info.</small></p>
      <?php } else { ?>
      <p class="text-center mt-3"><small>By chatting with <?=$chatName ?>, you state that you have read the <a href="tos.php">Terms of Service</a> and the <a href="gdpr.php">Privacy Policy</a>, and agree to both.</small></p>
      <?php }
      addon("chat_footer");
      } ?>
    </div>
  </div>
</div>
<a class="br" href="https://github.com/VoidXH/Poor-Man-s-AI"><img src="img/github.svg"></a>
<script>
const you = "<?=htmlspecialchars($uid ? $_COOKIE['username'] : 'You') ?>";
const gpt = "<?=htmlspecialchars($chatName) ?>";
</script>
<script src="<?=$jqueryPath ?>"></script>
<script src="<?=$bootstrapJSPath ?>"></script>
<script src="<?=$markedPath ?>"></script>
<script src="js/command.js"></script>
<script src="js/chat.js"></script>
</body>
</html>