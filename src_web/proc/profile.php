<?php if ($uid || $open) { ?>
<div class="input-group profile m-2">
<?php if ($uid) { ?>
	<a class="btn btn-secondary" href="profile.php"><?=htmlspecialchars($_COOKIE["username"]) ?></a>
	<a class="btn btn-secondary" href="logout.php">Logout</a>
<?php } else { ?>
	<a class="btn btn-primary" href="login.php">Login</a>
<?php } ?>
</div>
<?php } ?>
