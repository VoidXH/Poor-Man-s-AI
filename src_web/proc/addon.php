<?php
function addon($path) {
  global $sqlink, $admin, $uid;
  $path = __DIR__."/../addon/$path.php";
  if (file_exists($path)) {
    include($path);
  }
}
?>