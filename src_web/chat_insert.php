<?php
require_once("__config.php");
require_once("sql.php");
require_once("proc/ai_vars.php");

function aiChatInternal($pmaiPath, $name, $modelName, $fullInclude, $online) {
?>
<link rel="stylesheet" href="<?=$pmaiPath ?>/css/chat.css">
<?php if ($fullInclude) { ?>
<link rel="stylesheet" href="<?=$bootstrapPath ?>">
<?php }
require("__config.php");
if (time() - getAIVar("llm-available") <= $procTimeout) { ?>
<div id="model" style="display: none;">
  <button class="btn-primary"><?=$modelName ?></button>
</div>
<div class="card">
  <div class="card-header d-flex align-items-center">
    <span class="text-center flex-grow-1"><?=$name ?></span>
  </div>
  <div class="card-body chatbox">
    <p class="text-center mt-3"><small>By chatting with <?=$name ?>, you state that you have read the <a href="<?=$pmaiPath ?>/tos.php">Terms of Service</a> and the <a href="<?=$pmaiPath ?>/gdpr.php">Privacy Policy</a>, and agree to both.</small></p>
  </div>
  <div class="card-footer">
    <div class="input-group">
      <textarea class="form-control" id="input" placeholder="Ask <?=$name ?> anything..." autofocus></textarea>
      <div class="input-group-append">
        <button class="btn btn-danger" id="stop" style="display:none;" onclick="stop()">Stop</button>
        <button class="btn btn-primary" id="send" onclick="send()">Send</button>
      </div>
    </div>
  </div>
</div>
<script>
const you = "You";
const gpt = "<?=htmlspecialchars($name) ?>";
const pmaiPath = "<?=$pmaiPath ?>/";
</script>
<?php } ?>
<?php if ($fullInclude) { ?>
<script src="<?=$jqueryPath ?>"></script>
<script src="<?=$bootstrapJSPath ?>"></script>
<?php } ?>
<script src="<?=$markedPath ?>"></script>
<script src="<?=$pmaiPath ?>/js/command.js"></script>
<script src="<?=$pmaiPath ?>/js/chat.js"></script>
<?php
}

function aiChat($pmaiPath, $name, $modelName, $fullInclude) {
  aiChatInternal($pmaiPath, $name, $modelName, $fullInclude, time() - getAIVar("llm-available") <= 10);
}

function aiChatPopup($pmaiPath, $name, $modelName, $fullInclude) {
?>
<link rel="stylesheet" href="<?=$pmaiPath ?>/css/chat_insert.css">
<?php
  $online = time() - getAIVar("llm-available") <= 10;
  if ($online) { ?>
<button class="btn btn-secondary btn-lg btn-float br" onclick="$('#chatwin').show()">Chat with <?=$name ?></button>
<div class="chatwin" id="chatwin">
<?php aiChatInternal($pmaiPath, $name, $modelName, $fullInclude, $online); ?>
<button class="btn btn-danger btn-sm btn-float tr" onclick="$('#chatwin').hide()">X</button>
</div>
<?php }
}
?>