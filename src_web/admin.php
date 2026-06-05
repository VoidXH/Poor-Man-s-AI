<?php
require_once("proc/loading.php");
if (!$admin) {
    die;
}

$p = $_GET["p"] ?? 1;

function tabLink($i, $name) {
    global $p;
    $class = "nav-item nav-link";
    if ($p == $i) {
        $class .= " active";
    }
    return "<a class=\"$class\" href=\"?p=$i\">$name</a>";
}
?>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <?=$viewport ?>
    <title><?=$siteName ?> admin</title>
    <link rel="stylesheet" href="<?=$bootstrapPath ?>">
    <link rel="stylesheet" href="css/dark.css">
</head>
<body>
<div class="container">
    <div class="nav nav-tabs">
        <a class="nav-item nav-link" href="index.php" role="tab">Back</a>
        <?=tabLink(1, "Config") ?>
        <?=tabLink(2, "Var dump") ?>
        <?=tabLink(3, "Dislikes") ?>
        <?=tabLink(4, "Shell") ?>
        <?=tabLink(5, "Mode") ?>
    </div>
    <?php switch($p) {
        case 2:
            require("admin/var_dump.php");
            break;
        case 3:
            require("admin/dislikes.php");
            break;
        case 4:
            require("admin/shell.php");
            break;
        case 5:
            require("admin/mode.php");
            break;
        default:
            require("admin/config.php");
            break;
    } ?>
</div>
</body>
</html>