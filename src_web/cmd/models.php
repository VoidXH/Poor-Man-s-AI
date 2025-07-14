<?php
/*
  Model handling API endpoints
  ----------------------------
  Mode = EngineCacheMode in the Processor
  GET:
    ?active: get the active mode
  POST:
    active: set the active mode
*/

require("../_check.php");
require("../proc/ai_vars.php");

if (!$admin) {
    die;
}

if (isset($_GET["active"])) {
    echo getAIVar("mode");
} else if (isset($_POST["active"])) {
    setAIVar("mode", $_POST["active"]);
}
?>
