<?php
setcookie("username", "", time() - 3600, "/", "", false, true);
setcookie("password", "", time() - 3600, "/", "", false, true);
header("Location: index.php");
?>
