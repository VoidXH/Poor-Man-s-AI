<?php
require_once("sql.php");
require_once("proc/sqlink.php");
if (isset($_COOKIE["username"]) && isset($_COOKIE["password"])) {
  $stmt = execute("SELECT id, admin FROM ai_users WHERE name = ? AND password = ?", $_COOKIE["username"], $_COOKIE["password"]);
  $result = $stmt->get_result();
  if ($result->num_rows !== 1) require("login.php");
  $row = $result->fetch_assoc();
  $stmt->close();
  $admin = (bool)$row["admin"];
  $uid = $row["id"];
} else if ($open) {
  $uid = false;
} else {
  require("login.php");
}
?>
