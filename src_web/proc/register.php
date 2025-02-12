<?php
require_once("sqlink.php");

function register($name, $password) {
  global $sqlink;
  $salt = bin2hex(random_bytes(16));
  $hashed_password = hash("sha256", $salt . $password);
  $stmt = execute("INSERT INTO ai_users (name, password, salt) VALUES (?, ?, ?)", $name, $hashed_password, $salt);
  $stmt->close();
}
?>
