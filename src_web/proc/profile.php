<?php
$rootClass = $instantMode ? "btn btn-sm" : "btn";
$loginClass = $instantMode ? "w-auto" : "profile m-2";
if ($uid || $open) {
?>
<div class="input-group <?=$loginClass ?>">
<?php if ($uid) { ?>
	<a class="<?=$rootClass ?> btn-secondary" href="profile.php"><?=htmlspecialchars($_COOKIE["username"]) ?></a>
	<a class="<?=$rootClass ?> btn-secondary" href="logout.php">Logout</a>
<?php } else { ?>
	<a class="<?=$rootClass ?> btn-primary" href="login.php">Login</a>
<?php } ?>
</div>
<?php } ?>
