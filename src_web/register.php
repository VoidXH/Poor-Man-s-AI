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
  <link href="css/chat.css" rel="stylesheet">
  <link href="css/dark.css" rel="stylesheet">
</head>
<body>
  <div class="container d-flex align-items-center justify-content-center" style="min-height: 100vh;">
    <div class="card" style="height: auto; max-width: 450px; width: 100%;">
      <div class="card-body p-5">
        <h1 class="text-center mb-4 fw-bold" style="font-size: 2rem; letter-spacing: -0.02em;">Create Account</h1>
        
        <?php if ($exists) { ?>
        <div class="alert alert-danger mb-4" role="alert">
          <strong>Username taken</strong><br>
          This username is already in use. Please choose a different one.
        </div>
        <?php } else if ($noPriv) { ?>
        <div class="alert alert-danger mb-4" role="alert">
          <strong>Privacy Policy required</strong><br>
          You must accept the Privacy Policy to create an account.
        </div>
        <?php } else if ($noMatch) { ?>
        <div class="alert alert-danger mb-4" role="alert">
          <strong>Passwords don't match</strong><br>
          Please ensure both password fields are identical.
        </div>
        <?php } ?>

        <form method="POST">
          <div class="mb-3">
            <label for="name" class="form-label fw-500 mb-2">Username</label>
            <input 
              type="text" 
              class="form-control" 
              id="name"
              name="name" 
              placeholder="Choose your username"
              required
              autocomplete="username">
          </div>

          <div class="mb-3">
            <label for="password" class="form-label fw-500 mb-2">Password</label>
            <input 
              type="password" 
              class="form-control" 
              id="password"
              name="password" 
              placeholder="Enter a strong password"
              required
              autocomplete="new-password">
          </div>

          <div class="mb-4">
            <label for="password2" class="form-label fw-500 mb-2">Confirm Password</label>
            <input 
              type="password" 
              class="form-control" 
              id="password2"
              name="password2" 
              placeholder="Re-enter your password"
              required
              autocomplete="new-password">
          </div>

          <div class="form-check mb-4">
            <input 
              class="form-check-input" 
              type="checkbox" 
              id="priv" 
              name="priv" 
              required>
            <label class="form-check-label" for="priv" style="font-size: 0.9rem; color: var(--text-secondary);">
              I have read and agree to the <a href="tos.php" style="color: var(--accent); text-decoration: none;">Terms of Service</a> 
              and the <a href="gdpr.php" style="color: var(--accent); text-decoration: none;">Privacy Policy</a>
            </label>
          </div>

          <button type="submit" class="btn btn-primary w-100 fw-600 py-2 mb-3" style="font-size: 0.95rem;">
            Create Account
          </button>
        </form>

        <div class="text-center" style="font-size: 0.9rem; color: var(--text-secondary);">
          Already have an account? <a href="login.php" style="color: var(--accent); text-decoration: none;">Sign in</a>
        </div>
      </div>
    </div>
  </div>
</body>
</html>
