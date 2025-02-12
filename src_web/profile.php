<?php
require("__config.php");
require("_check.php");
if (!$uid) {
  header("Location: index.php");
  die;
}

$name = $_COOKIE["username"];
$pass = $_COOKIE["password"];
if (isset($_POST["new"])) {
  $new = $_POST["new"];
  $new2 = $_POST["new2"];
  if ($new != $new2) {
    $match = true;
  } else {
    $result = $sqlink->query("SELECT name, password, salt FROM ai_users WHERE id = $uid")->fetch_assoc();
    if ($result["name"] != $name || $result["password"] != $pass) die;

    $current = hash("sha256", $result["salt"] . $_POST["current"]);
    if ($current != $result["password"]) {
      $wrong = true;
    } else {
      $hash = hash('sha256', $result['salt'] . $new);
      if ($sqlink->query("UPDATE ai_users SET password = \"$hash\" WHERE id = $uid")) {
        header("Location: login.php?pass");
        die;
      }
    }
  }
}

$deleteKey = hash("sha256", $name.$pass);
if ($_POST["key"] == $deleteKey && $sqlink->query("DELETE FROM ai_users WHERE id = $uid")) {
  header("Location: login.php?del");
  die;
}
?>
<html>
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title>Profile</title>
  <link href="<?=$bootstrapPath ?>" rel="stylesheet">
  <link href="css/dark.css" rel="stylesheet">
  <link href="css/user.css" rel="stylesheet">
</head>
<body>
  <div class="container">
    <a href="index.php" class="btn btn-primary back mt-3">Back</a>
    <div class="login mt-3">
      <form method="POST">
        <h2>Change password</h2>
        <?php if ($match) { ?>
        <div class="alert alert-danger" role="alert">The new passwords didn't match.</div>
        <?php } else if ($wrong) { ?>
        <div class="alert alert-danger" role="alert">The current password was incorrect.</div>
        <?php } ?>
        <div class="form-group">
          <label for="current">Current password</label>
          <input type="password" class="form-control" name="current" required>
        </div>
        <div class="form-group">
          <label for="new">New password</label>
          <input type="password" class="form-control" name="new" required>
        </div>
        <div class="form-group">
          <label for="new2">New password again</label>
          <input type="password" class="form-control" name="new2" required>
        </div>
        <button type="submit" class="btn btn-primary mr-3">Change password</button>
      </form>
      <div class="register">
        <form method="POST" onsubmit="return confirm('Do you really want to delete your account?')">
          <input type="hidden" name="key" value="<?=$deleteKey ?>">
          <button type="submit" class="btn btn-danger mb-3">Delete account</button>
        </form>
      </div>
    </div>
  </div>
</body>
</html>
<?php die; ?>
