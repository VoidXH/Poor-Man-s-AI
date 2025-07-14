<?php
require("__config.php");
require_once("sql.php");
require("proc/register.php");

if (!$regOn) {
  header("Location: index.php");
  die;
}

if (isset($_POST["name"])) {
  global $sqlink;
  $name = $_POST["name"];
  $password = $_POST["password"];
  $password2 = $_POST["password2"];
  if (!$_POST["priv"]) {
    $noPriv = true;
  } else if ($password != $password2) {
    $noMatch = true;
  } else {
    $stmt = execute("SELECT * FROM ai_users WHERE name = ?", $name);
    $result = $stmt->get_result();
    $stmt->close();
    if ($result->num_rows > 0) {
      $exists = true;
    } else {
      register($name, $password);
      header("Location: login.php?reg");
      die;
    }
  }
}
?>
<html>
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title>Registration</title>
  <link href="<?=$bootstrapPath ?>" rel="stylesheet">
  <link href="css/dark.css" rel="stylesheet">
  <link href="css/user.css" rel="stylesheet">
</head>
<body>
  <div class="container mt-5">
    <?php if ($exists) { ?>
    <div class="alert alert-danger" role="alert">This username is already used.</div>
    <?php } else if ($noPriv) { ?>
    <div class="alert alert-danger" role="alert">Accounts can only be created if you accept the Privacy Policy.</div>
    <?php } else if ($noMatch) { ?>
    <div class="alert alert-danger" role="alert">The passwords didn't match.</div>
    <?php } ?>
    <div class="login">
      <form method="POST">
        <h2>Registration</h2>
        <div class="form-group mb-2">
          <label for="name">Username</label>
          <input type="text" class="form-control" name="name" required>
        </div>
        <div class="form-group mb-2">
          <label for="password">Password</label>
          <input type="password" class="form-control" name="password" required>
        </div>
        <div class="form-group mb-2">
          <label for="password2">Repeat password</label>
          <input type="password" class="form-control" name="password2" required>
        </div>
        <input type="checkbox" id="priv" name="priv">
        <label for="priv" style="display: contents;">I have read the <a href="tos.php">Terms of Service</a> and the <a href="gdpr.php">Privacy Policy</a>, and agree to both.</label><br>
        <button type="submit" class="btn btn-primary mt-3 mb-3">Register</button>
      </form>
    </div>
  </div>
</body>
</html>
