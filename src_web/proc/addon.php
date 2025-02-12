<?php
function addon($path) {
  global $admin, $uid;
  $path = "addon/$path.php";
  if (file_exists($path)) {
    include($path);
  }
}
?>
