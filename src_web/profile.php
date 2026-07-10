<?php
require('_check.php');
if (!$uid) {
  header('Location: index.php');
  die;
}

$name = $_COOKIE['username'];
$pass = $_COOKIE['password'];
if (isset($_POST['new'])) {
  $new = $_POST['new'];
  $new2 = $_POST['new2'];
  if ($new != $new2) {
    $match = true;
  } else {
    $result = $sqlink->query("SELECT name, password, salt FROM ai_users WHERE id = $uid")->fetch_assoc();
    if ($result['name'] != $name || $result['password'] != $pass) die;

    $current = hash('sha256', $result['salt'] . $_POST['current']);
    if ($current != $result['password']) {
      $wrong = true;
    } else {
      $hash = hash('sha256', $result['salt'] . $new);
      if ($sqlink->query("UPDATE ai_users SET password = \"$hash\" WHERE id = $uid")) {
        header('Location: login.php?pass');
        die;
      }
    }
  }
}

$deleteKey = hash('sha256', $name.$pass);
if ($_POST['key'] == $deleteKey && $sqlink->query("DELETE FROM ai_users WHERE id = $uid")) {
  header('Location: login.php?del');
  die;
}

$prompts = $sqlink->query("SELECT prompts FROM ai_users WHERE id = $uid")->fetch_assoc()['prompts'];
?>
<html class="theme-<?=$theme ?>">
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title>Profile</title>
  <link href="<?=$bootstrapPath ?>" rel="stylesheet">
  <link href="css/dark.css" rel="stylesheet">
  <link href="css/chat.css" rel="stylesheet">
</head>
<body>
  <div class="container">
    <a href="index.php" class="btn btn-secondary back mt-3">← Back</a>
    <div class="card h-auto mt-4">
      <div class="card-header d-flex align-items-center gap-3">
        <div class="rounded-circle d-flex align-items-center justify-content-center" style="width:48px;height:48px;background:var(--accent);font-size:1.25rem;font-weight:700;color:#0a0e14;"><?=strtoupper(substr($name, 0, 1))?></div>
        <h5 class="mb-0"><?=$name?></h5>
      </div>
      <div class="card-body">
        <div class="d-flex align-items-center gap-2 mb-1">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="var(--accent)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
          <span style="color:var(--text-secondary)">Prompts written:</span>
          <strong style="color:var(--accent);font-size:1.1rem"><?=$prompts?></strong>
        </div>
      </div>
    </div>

    <div class="card h-auto mt-3">
      <div class="card-header">
        <span>Change password</span>
      </div>
      <div class="card-body">
        <?php if ($match) { ?>
        <div class="alert alert-danger" role="alert">The new passwords didn't match.</div>
        <?php } else if ($wrong) { ?>
        <div class="alert alert-danger" role="alert">The current password was incorrect.</div>
        <?php } ?>
        <form method="POST">
          <div class="mb-3">
            <label class="form-label">Current password</label>
            <input type="password" class="form-control" name="current" placeholder="Enter current password" required>
          </div>
          <div class="mb-3">
            <label class="form-label">New password</label>
            <input type="password" class="form-control" name="new" placeholder="Enter new password" required>
          </div>
          <div class="mb-4">
            <label class="form-label">Confirm new password</label>
            <input type="password" class="form-control" name="new2" placeholder="Confirm new password" required>
          </div>
          <button type="submit" class="btn btn-primary">Change password</button>
        </form>
      </div>
    </div>

    <div class="card h-auto mt-3">
      <div class="card-header">
        <span class="text-danger">Delete account</span>
      </div>
      <div class="card-body">
        <p style="color:var(--text-secondary);margin-bottom:1rem">Once deleted, your account and all associated data cannot be recovered.</p>
        <form method="POST" onsubmit="return confirm('Do you really want to delete your account?')">
          <input type="hidden" name="key" value="<?=$deleteKey ?>">
          <button type="submit" class="btn btn-danger">Delete account</button>
        </form>
      </div>
    </div>
  </div>
</body>
</html>
<?php die; ?>
