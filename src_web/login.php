<?php
require("__config.php");
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
    $hashed_password = hash("sha256", $salt . $password);
    if ($hashed_password === $db_password) {
      setcookie("username", $name, time() + 7 * 24 * 60 * 60, "/", "", false, true);
      setcookie("password", $hashed_password, time() + 7 * 24 * 60 * 60, "/", "", false, true);
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
  <link href="css/user.css" rel="stylesheet">
</head>
<body>
  <div class="container mt-5">
    <div class="login">
      <form method="POST">
        <h2>Login</h2>
        <?php if ($failed) { ?>
        <div class="alert alert-danger" role="alert">Invalid credentials.</div>
        <?php } else if (isset($_GET["reg"])) { ?>
        <div class="alert alert-success" role="alert">Registration successful, you can now log in.</div>
        <?php } else if (isset($_GET["pass"])) { ?>
        <div class="alert alert-success" role="alert">Password change successful, please log in again.</div>
        <?php } else if (isset($_GET["del"])) { ?>
        <div class="alert alert-success" role="alert">Account deleted successfully.</div>
        <?php } ?>
        <div class="form-group">
          <label for="name">Username</label>
          <input type="text" class="form-control" name="name" required>
        </div>
        <div class="form-group">
          <label for="password">Password</label>
          <input type="password" class="form-control" name="password" required>
        </div>
        <button type="submit" class="btn btn-primary mb-3">Login</button>
      </form>
      <?php if ($regOn) { ?>
      <div class="register">
        <h2>No account?</h2>
        <a href="register.php" class="btn btn-secondary mb-3">Register here!</a>
      </div>
      <?php } ?>
    </div>
  </div>
</body>
</html>
<?php die; ?>
