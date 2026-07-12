<?php
require_once("proc/loading.php");
if (!$admin) {
    die;
}

$p = $_GET["p"] ?? 1;

function tabLink($i, $name) {
    global $p;
    $class = "btn btn-sm ";
    if ($p == $i) {
        $class .= "btn-primary";
    } else {
        $class .= "btn-secondary";
    }
    return "<a class=\"$class\" href=\"?p=$i\">$name</a>";
}
?>
<html lang="en" class="theme-<?=$theme ?>">
<head>
    <meta charset="UTF-8">
    <?=$viewport ?>
    <title><?=$siteName ?> Admin</title>
    <link rel="stylesheet" href="<?=$bootstrapPath ?>">
    <link rel="stylesheet" href="css/moa.css">
    <link rel="stylesheet" href="css/dark.css">
    <link rel="stylesheet" href="css/chat.css">
</head>
<body>
<div class="container">
  <div class="card">
    <div class="card-header d-flex align-items-center">
<?php require('proc/chat/menu.php'); ?>
      <span class="text-center flex-grow-1">Admin</span>
    </div>
    <div class="card-body d-flex flex-column" style="flex: 1; min-height: 0;">
      <div class="d-flex flex-wrap gap-2 mb-3">
        <?=tabLink(1, "Config") ?>
        <?=tabLink(2, "Var Dump") ?>
        <?=tabLink(3, "Dislikes") ?>
        <?=tabLink(4, "Mode") ?>
        <?=tabLink(5, "Usage") ?>
      </div>
      <div class="flex-grow-1 overflow-auto" style="min-height: 0;">
<?php switch($p) {
    case 2:
        require("admin/var_dump.php");
        break;
    case 3:
        require("admin/dislikes.php");
        break;
    case 4:
        require("admin/mode.php");
        break;
    case 5:
        require("admin/usage.php");
        break;
    default:
        require("admin/config.php");
        break;
} ?>
      </div>
    </div>
  </div>
</div>
<a class="br" href="https://github.com/VoidXH/Poor-Man-s-AI"><img src="img/github.svg"></a>
<script src="<?=$jqueryPath ?>"></script>
<script src="<?=$bootstrapJSPath ?>"></script>
<script src="js/command.js"></script>
<script src="js/admin.js"></script>
<script src="js/menu.js"></script>
</body>
</html>