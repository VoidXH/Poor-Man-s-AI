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
					<li><a class="dropdown-item" onclick="send(<?= $agentQueueByDefault ? 'false' : 'true' ?>)"><?= $agentQueueByDefault ? 'Perform now' : 'Queue prompt' ?></a></li>
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
			<div class="mb-2">
				<div class="dropdown d-inline" id="agent-dropdown">
						<button class="btn btn-sm btn-secondary dropdown-toggle" type="button" id="agentDropdownBtn" data-bs-toggle="dropdown" aria-expanded="false">
							<span id="agentLabel"></span>
						</button>
						<ul class="dropdown-menu" id="agentMenu">
<?php
	$agents = preg_split('/\s*,\s*/', $agentModels);
	foreach ($agents as $index => $agent) {
			echo "<li><a class=\"dropdown-item\" href=\"#\" data-agent=\"".$agent."\">".$agent."</a></li>\n";
	}
?>
						</ul>
					</div>
				<div class="dropdown d-inline" id="path-dropdown">
						<button class="btn btn-sm btn-primary dropdown-toggle" type="button" id="pathDropdownBtn" data-bs-toggle="dropdown" aria-expanded="false">
							<span id="pathLabel"></span>
						</button>
						<ul class="dropdown-menu" id="pathMenu">
<?php
	$paths = preg_split('/\s*,\s*/', $agentPaths);
	foreach ($paths as $index => $path) {
			$label = basename(str_replace('\\', '/', $path));
			echo "<li><a class=\"dropdown-item\" href=\"#\" data-path=\"".$path."\">".$label."</a></li>\n";
	}
?>
						</ul>
					</div>
			</div>
			<div class="file-input-container" id="file-blocks-container"></div>
				<div class="input-group">
				<textarea class="form-control" id="input" placeholder="Give <?=$chatName ?> a task..." autofocus></textarea>
				<button class="btn btn-danger" id="stop" style="display:none;" onclick="stop()">Stop</button>
				<button class="btn btn-primary" id="send" onclick="send()"><?= $agentQueueByDefault ? 'Queue' : 'Send' ?></button>
			</div>
<?php } ?>
		</div>
	</div>
</div>
<script>
const you = "<?=htmlspecialchars($_COOKIE['username']) ?>";
const agentQueueByDefault = <?= $agentQueueByDefault ? 'true' : 'false' ?>;
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
