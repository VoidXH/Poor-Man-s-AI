<?php
require("__config.php");
require("_check.php");
require("proc/ai_vars.php");

$time = time();
$offline = $time - getAIVar("llm-available") > 10;
$slm = $time - getAIVar("moa-available") <= 10;
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
        <div class="input-group-append" id="model">
          <button class="btn btn-primary" id="chat">Chat</button>
          <button class="btn btn-secondary" id="think">Think</button>
          <button class="btn btn-secondary" id="code">Code</button>
        </div>
      </div>
    </div>
    <div class="card-body chatbox">
      <?php if ($slm) { ?><div class="alert alert-warning" role="alert">The server is currently running the chat on CPU. The quality and generation speed of answers may be worse.</div>
      <?php } else if ($offline) { ?><div class="alert alert-danger" role="alert">Shhh! The chatting computer is sleeping and can't work now. Wait until it wakes up!</div><?php } ?>
    </div>
    <div class="card-footer">
      <?php if (!$offline) { ?>
      <div class="input-group">
        <textarea class="form-control" id="input" placeholder="Message <?=$chatName ?>..."></textarea>
        <div class="input-group-append">
          <button class="btn btn-danger" id="stop" style="display:none;" onclick="stop()">Stop</button>
          <button class="btn btn-primary" id="send" onclick="send()">Send</button>
        </div>
      </div>
      <?php }
      if ($uid) { ?>
      <p class="text-center mt-3"><small><?=$chatName ?> can make mistakes. Check important info.</small></p>
      <?php } else { ?>
      <p class="text-center mt-3"><small>By chatting with <?=$chatName ?>, you state that you have read the <a href="tos.php">Terms of Service</a> and the <a href="gdpr.php">Privacy Policy</a>, and agree to both.</small></p>
      <?php } ?>
    </div>
  </div>
</div>
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
