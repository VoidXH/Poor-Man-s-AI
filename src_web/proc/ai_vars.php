<?php
require_once("sqlink.php");

function getAIVar($key) {
  global $sqlink;
  $stmt = execute("SELECT value FROM ai_vars WHERE `key` = ?", $key);
  $stmt->bind_result($value);
  $stmt->fetch();
  $stmt->close();
  return $value;
}

function setAIVar($key, $value) {
  global $sqlink;
  $stmt = execute("UPDATE ai_vars SET `value` = ? WHERE `key` = ?", $value, $key);
  $stmt->close();
}
?>
