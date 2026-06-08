<?php
require_once("__config.php");
require_once("proc/sqlink.php");
require_once("sql.php");

if (isset($_POST["name"])) {
	$name = $_POST["name"];
	$password = $_POST["password"];
	$stmt = execute("SELECT password, salt FROM ai_users WHERE name = ?", $name);
	$stmt->store_result();
	if ($stmt->num_rows > 0) {
		$stmt->bind_result($db_password, $salt);
		$stmt->fetch();
		$stmt->close();
		$hashedPass = hash("sha256", $salt . $password);
		if ($hashedPass === $db_password) {
			$refresh = time() + $loginTimeout;
			setcookie("username", $name, $refresh, "/", "", false, true);
			setcookie("password", $hashedPass, $refresh, "/", "", false, true);
			header("Location: index.php");
			die;
		} else {
			$failed = true;
		}
	} else {
		$failed = true;
	}
}
?>
<html>
<head>
	<meta charset="UTF-8">
	<?=$viewport ?>
	<title>Login</title>
	<link href="<?=$bootstrapPath ?>" rel="stylesheet">
	<link href="css/dark.css" rel="stylesheet">
	<link href="css/chat.css" rel="stylesheet">
</head>
<body class="d-flex align-items-center justify-content-center" style="min-height: 100vh;">
	<div class="container">
		<div class="row justify-content-center">
			<div class="col-12 col-sm-10 col-md-8 col-lg-5">
				<div class="card">
					<div class="card-body p-5">
						<h2 class="text-center mb-4 fw-bold" style="letter-spacing: -0.01em;">Access <?=$siteName ?></h2>
<?php if ($failed) { ?>
						<div class="alert alert-danger" role="alert">
							<strong>Invalid credentials.</strong> Please check your username and password.
						</div>
<?php } else if (isset($_GET["reg"])) { ?>
						<div class="alert alert-success" role="alert">
							<strong>Success!</strong> Registration complete. You can now log in.
						</div>
<?php } else if (isset($_GET["pass"])) { ?>
						<div class="alert alert-success" role="alert">
							<strong>Success!</strong> Password updated. Please log in again.
						</div>
<?php } else if (isset($_GET["del"])) { ?>
						<div class="alert alert-success" role="alert">
							<strong>Success!</strong> Account deleted.
						</div>
<?php } ?>
						
						<form method="POST" class="needs-validation">
							<div class="mb-4">
								<label for="name" class="form-label fw-500">Username</label>
								<input type="text" class="form-control" id="name" name="name" placeholder="Enter your username" required autofocus>
							</div>
							
							<div class="mb-4">
								<label for="password" class="form-label fw-500">Password</label>
								<input type="password" class="form-control" id="password" name="password" placeholder="Enter your password" required>
							</div>
							
							<button type="submit" class="btn btn-primary w-100 fw-600 py-2 mb-3">
								<span>Sign In</span>
							</button>
						</form>
						
<?php if ($regOn) { ?>
						<div class="text-center pt-3 border-top" style="border-color: var(--glass-border) !important;">
							<p class="text-secondary mb-3" style="color: var(--text-secondary); font-size: 0.9rem;">Don't have an account?</p>
							<a href="register.php" class="btn btn-secondary w-100 fw-500 py-2">Create Account</a>
						</div>
<?php } ?>
					</div>
				</div>
			</div>
		</div>
	</div>
</body>
</html>
<?php die; ?>
