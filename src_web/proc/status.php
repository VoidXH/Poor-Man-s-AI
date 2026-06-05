<?php
function online() {
	global $online; ?>
<span class="input-group-text bg-<?=$online ? "success" : "danger" ?> text-white"><?=$online ? "On" : "Off" ?>line</span>
<?php } ?>
<div class="server input-group mt-2">
<?php if ($admin) {
	online();
	addon("adminbtn");
?>
	<a class="btn btn-secondary" href="admin.php">Admin</a>
<?php
	if ($online) {
?>
	<div class="btn btn-primary m-0 p-0 dropdown show">
		<a class="btn btn-primary dropdown-toggle" href="#" role="button" id="modeList" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">Mode</a>
		<div class="dropdown-menu" aria-labelledby="modeList">
		<a class="<?=$moaClass ?> dropdown-item" onclick="enableMoA()">Image + SLM</a>
		<a class="<?=$llmClass ?> dropdown-item" onclick="enableLLM()">LLM</a>
	</div>
</div>
<?php
	}
} else {
	online();
}
?>
</div>
