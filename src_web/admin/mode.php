<?php
if (!$admin) {
	die;
}

$time = time();
$online = $time - getAIVar("llm-available") <= 10 || $time - getAIVar("moa-available") <= 10;
$mode = getAIVar("mode");
$moaClass = $mode == 5 ? "active" : "";
$llmClass = $mode == 2 ? "active" : "";

function modeButton($name, $value, $activeClass) {
	global $online;
	$disabled = $online ? '' : 'disabled';
	$class = $activeClass ? 'btn-primary' : 'btn-outline-primary';
	echo '<button class="btn '.$class.'" onclick="modeSwap('.$value.')" '.$disabled.'>'.$name.'</button>';
}
?>
<h2>Mode Selector</h2>
<?php if (!$online) { ?>
<div class="alert alert-warning mt-3">
	The Processor is currently <strong>offline</strong>. Mode switching is disabled until it comes back online.
</div>
<?php } ?>
<div class="row mt-3" style="gap: 1rem;">
	<div class="col-md-4">
		<div class="card <?= $moaClass ? 'border-primary' : '' ?>">
			<div class="card-header">Image + SLM</div>
			<div class="card-body">
				<p class="card-text">Multimodal mode using a Small Language Model for chat and image generation capabilities.</p>
				<p class="mb-0"><strong>Mode ID:</strong> 5</p>
			</div>
		</div>
	</div>
	<div class="col-md-4">
		<div class="card <?= $llmClass ? 'border-primary' : '' ?>">
			<div class="card-header">LLM</div>
			<div class="card-body">
				<p class="card-text">Pure language model mode for text-based chat only.</p>
				<p class="mb-0"><strong>Mode ID:</strong> 2</p>
			</div>
		</div>
	</div>
</div>
<div class="mt-3">
	<button class="btn btn-primary" onclick="modeSwap(5)" <?= $online ? '' : 'disabled' ?>>Switch to Image + SLM</button>
	<button class="btn btn-primary" onclick="modeSwap(2)" <?= $online ? '' : 'disabled' ?>>Switch to LLM</button>
</div>
<div id="mode-status" class="mt-3"></div>
<script src="<?=$jqueryPath ?>"></script>
<script src="js/admin.js"></script>
<script src="js/command.js"></script>
<script src="js/index.js"></script>
