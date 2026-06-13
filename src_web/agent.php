<?php
require_once('proc/loading.php');
if (!$admin) {
	die;
}

$time = time();
$offline = $time - getAIVar("agent-available") > $procTimeout;
?>
<html lang="en">
<head>
	<meta charset="UTF-8">
	<?=$viewport ?>
	<title>Agent <?=$chatName ?></title>
	<link rel="stylesheet" href="<?=$bootstrapPath ?>">
	<link rel="stylesheet" href="css/dark.css">
</head>
<body>
<link rel="stylesheet" href="css/chat.css">
<div class="container">
	<div class="card">
		<div class="card-header d-flex align-items-center">
<?php
	require('proc/chat/menu.php');
?>
			<span class="text-center flex-grow-1">Agent <?=$chatName ?></span>
			<div class="dropdown">
				<button class="btn btn-sm btn-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
					<span class="fas fa-terminal"></span>
				</button>
				<ul class="dropdown-menu dropdown-menu-end">
					<li><a class="dropdown-item" onclick="sendCommandByPrompt('Files')">List files</a></li>
					<li><a class="dropdown-item" onclick="sendCommandByPrompt('GitStatus')">List changed files</a></li>
					<li><a class="dropdown-item" onclick="sendCommandByPrompt('GitDiff')">Show all changes</a></li>
        			<li><hr class="dropdown-divider"></li>
					<li><a class="dropdown-item" onclick="sendCommandByPrompt('Queue:' + $('#input').val())">Queue prompt</a></li>
					<li><a class="dropdown-item" onclick="sendCommandByPrompt('Queue')">Show queue</a></li>
					<li><a class="dropdown-item" onclick="sendCommandByPrompt('QueueClear')">Clear queue</a></li>
        			<li><hr class="dropdown-divider"></li>
					<li><a class="dropdown-item" onclick="sendCommandByPrompt('Scrum:' + $('#input').val())">Perform in Scrum Mode</a></li>
				</ul>
			</div>
		</div>
		<div class="chat">
			<div id="display">
<?php if ($offline) { ?>
				<div class="alert alert-danger" role="alert">No agent-running Processor is online.</div>
<?php } ?>
			</div>
<?php require('proc/chat/disclaimer.php'); ?>
    	</div>
		<div class="card-footer">
<?php if (!$offline) { ?>
			<div id="path" class="target mb-1">
<?php
	$paths = preg_split('/\s*,\s*/', $agentPaths);
	foreach ($paths as $index => $path) {
		$class = ($index == 0) ? "btn btn-primary btn-sm" : "btn btn-secondary btn-sm";
		echo "<button class=\"$class\" id=\"path_$index\">$path</button>\n";
	}
?>
			</div>
			<div class="file-input-container" id="file-blocks-container"></div>
			<div class="input-group">
				<textarea class="form-control" id="input" placeholder="Give <?=$chatName ?> a task..." autofocus></textarea>
				<button class="btn btn-danger" id="stop" style="display:none;" onclick="stop()">Stop</button>
				<button class="btn btn-primary" id="send" onclick="send()">Send</button>
			</div>
<?php } ?>
		</div>
	</div>
</div>
<script>
const you = "<?=htmlspecialchars($_COOKIE['username']) ?>";
</script>
<script src="<?=$jqueryPath ?>"></script>
<script src="<?=$bootstrapJSPath ?>"></script>
<script src="<?=$markedPath ?>"></script>
<script src="<?=$highlightPath ?>"></script>
<script src="js/agent.js"></script>
<script src="js/command.js"></script>
<link rel="stylesheet" href="<?=$highlightCSSPath ?>">
<a class="br" href="https://github.com/VoidXH/Poor-Man-s-AI"><img src="img/github.svg"></a>
</body>
</html>
