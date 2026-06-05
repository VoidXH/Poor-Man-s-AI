<?php
$standalone = !isset($chatName);
require_once('proc/loading.php');

$time = time();
$offline = $time - getAIVar("llm-available") > $procTimeout;
$slm = $time - getAIVar("moa-available") <= $procTimeout;

if ($standalone) {
?>
<html lang="en">
<head>
	<meta charset="UTF-8">
	<?=$viewport ?>
	<title><?=$chatName ?></title>
	<link rel="stylesheet" href="<?=$bootstrapPath ?>">
	<link rel="stylesheet" href="css/dark.css">
</head>
<body>
<?php } ?>
<link rel="stylesheet" href="css/chat.css">
<div class="container">
	<div class="card">
		<div class="card-header d-flex align-items-center">
<?php if ($standalone) { ?>
			<a class="btn btn-sm btn-primary" href="index.php">Back</a>
<?php
} else {
			require('proc/profile.php');
} ?>
			<span class="text-center flex-grow-1"><?=$chatName ?></span>
			<button class="btn btn-sm btn-danger" id="reset" onclick="reset()">Reset</button>
		</div>
		<div class="card-body chat">
			<div class="chatbox">
<?php
if ($offline) {
?>
				<div class="alert alert-danger" role="alert">Shhh! The chatting computer is sleeping and can't work now. Wait until it wakes up!</div>
<?php
} else {
	if ($slm && $slmWarning) {
?>
				<div class="alert alert-warning" role="alert">The server is currently running the chat on CPU. The quality and generation speed of answers may be worse.</div>
<?php
	}
	if (isset($_POST["fill"])) {
		require('proc/chat/fill.php');
	} else {
		require('proc/chat/starters.php');
	}
} ?>
			</div>
<?php require('proc/chat/disclaimer.php'); ?>
    	</div>
		<div class="card-footer">
<?php
if (!$offline) {
?>
			<div id="model" class="mb-1">
<?php
	$models = preg_split('/\s*,\s*/', $chatModels);
	foreach ($models as $index => $model) {
		$class = ($index == 0) ? "btn btn-primary btn-sm" : "btn btn-secondary btn-sm";
		$id = strtolower($model);
		echo "<button class=\"$class\" id=\"$id\">$model</button>\n";
	}
?>
			</div>
			<div class="input-group">
				<textarea class="form-control" id="input" placeholder="Ask <?=$chatName ?> anything..." autofocus></textarea>
				<button class="btn btn-danger" id="stop" style="display:none;" onclick="stop()">Stop</button>
				<button class="btn btn-primary" id="send" onclick="send()">Send</button>
			</div>
<?php
	addon('chat_footer');
}
?>
		</div>
	</div>
</div>
<script>
const you = "<?=htmlspecialchars($uid ? $_COOKIE['username'] : 'You') ?>";
const gpt = "<?=htmlspecialchars($chatName) ?>";
</script>
<script src="<?=$jqueryPath ?>"></script>
<script src="<?=$markedPath ?>"></script>
<script src="js/chat.js"></script>
<?php if ($standalone) { ?>
<script src="<?=$bootstrapJSPath ?>"></script>
<script src="js/command.js"></script>
<a class="br" href="https://github.com/VoidXH/Poor-Man-s-AI"><img src="img/github.svg"></a>
</body>
</html>
<?php } ?>
