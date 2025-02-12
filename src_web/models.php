<?php
/*
  Model handling API endpoints
  ----------------------------
  GET:
    ?active: get the active mode (EngineCacheMode corresponding)
*/

require("_check.php");
require("proc/ai_vars.php");

if (!$admin) die;

if (isset($_GET["active"])) {
  die(getAIVar("mode"));
}

if (isset($_POST["active"])) {
  setAIVar("mode", $_POST["active"]);
  die;
}
?>
