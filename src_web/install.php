<?php
if (file_exists("sql.php")) {
  die("Poor Man's AI is already installed.");
}
require("__config.php");

$install = [
"DROP TABLE IF EXISTS `ai_commands`",
"CREATE TABLE `ai_commands` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `command` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `command_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `result` text COLLATE utf8mb4_unicode_ci,
  `result_ts` timestamp NULL DEFAULT NULL,
  `progress` int(11) DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;",
"DROP TABLE IF EXISTS `ai_users`",
"CREATE TABLE `ai_users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(32) NOT NULL,
  `password` char(64) NOT NULL,
  `salt` char(32) NOT NULL,
  `prompts` int(11) NOT NULL,
  `admin` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8",
"DROP TABLE IF EXISTS `ai_vars`",
"CREATE TABLE `ai_vars` (
  `key` varchar(32) NOT NULL,
  `value` varchar(32) NOT NULL,
  PRIMARY KEY (`key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8",
"INSERT INTO `ai_vars` (`key`, `value`) VALUES
('llm-available',	'0'),
('llm-weight',	'0'),
('moa-available',	'0'),
('moa-weight',	'0'),
('mode',	'5'),
('unreg-use',	'0')"
];

if ($_POST["db_db"]) {
  $db_host = $_POST["db_host"];
  $db_name = $_POST["db_name"];
  $db_pass = $_POST["db_pass"];
  $db_db = $_POST["db_db"];
  $admin_pass = $_POST["admin_pass"];
  $admin_pass2 = $_POST["admin_pass2"];
  $sqlink = new mysqli($db_host, $db_name, $db_pass, $db_db);
  if ($admin_pass != $admin_pass2) {
    $pwMatch = true;
  } else if ($sqlink->connect_errno) {
    $sqlogin = true;
  } else {
    $sqlink->set_charset('utf8');
    $error = false;
    foreach ($install as $cmd) {
      $sqlink->query($cmd);
      if ($sqlink->error) {
        $error = $sqlink->error;
        break;
      }
    }
    if (!$error) {
      require("proc/register.php");
      register($_POST["admin_name"], $admin_pass);
      $sqlink->query("UPDATE ai_users SET admin = 1");
      $sqlPhp = '<'.'?php
$sqlink = new mysqli("'.$db_host.'", "'.$db_name.'", "'.$db_pass.'", "'.$db_db.'");
$sqlink->set_charset("utf8");
?'.'>';
      file_put_contents('sql.php', $sqlPhp);
      header("Location: index.php");
      die;
    }
  }
}
?>
<html>
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title>Installation</title>
  <link href="<?=$bootstrapPath ?>" rel="stylesheet">
  <link href="css/dark.css" rel="stylesheet">
  <link href="css/user.css" rel="stylesheet">
</head>
<body>
  <div class="container mt-5">
    <?php if ($pwMatch) { ?>
    <div class="alert alert-danger" role="alert">The admin passwords didn't match.</div>
    <?php } else if ($sqlogin) { ?>
    <div class="alert alert-danger" role="alert">The database credentials and details were incorrect.</div>
    <?php } else if (isset($error)) { ?>
    <div class="alert alert-danger" role="alert">SQL error:<br><?=$error ?></div>
    <?php } ?>
    <div class="login">
      <form method="POST">
        <h2>Installation</h2>
        <h4 class="mt-5">Database</h4>
        <div class="form-group">
          <label for="db_host">Database server (hostname or IP)</label>
          <input type="text" class="form-control" name="db_host" value="<?=$_POST["db_host"] ?>" required>
        </div>
        <div class="form-group">
          <label for="db_name">Database username</label>
          <input type="text" class="form-control" name="db_name" value="<?=$_POST["db_name"] ?>" required>
        </div>
        <div class="form-group">
          <label for="db_pass">Database password</label>
          <input type="password" class="form-control" name="db_pass" required>
        </div>
        <div class="form-group">
          <label for="db_db">Database</label>
          <input type="text" class="form-control" name="db_db" value="<?=$_POST["db_db"] ?>" required>
        </div>
        <h4 class="mt-5">Admin user</h4>
        <div class="form-group">
          <label for="admin_name">Admin username</label>
          <input type="text" class="form-control" name="admin_name" value="<?=$_POST["admin_name"] ?>" required>
        </div>
        <div class="form-group">
          <label for="admin_pass">Admin password</label>
          <input type="password" class="form-control" name="admin_pass" required>
        </div>
        <div class="form-group">
          <label for="admin_pass2">Repeat admin password</label>
          <input type="password" class="form-control" name="admin_pass2" required>
        </div>
        <button type="submit" class="btn btn-primary mb-3">Install</button>
      </form>
    </div>
  </div>
</body>
</html>
