<?php
require_once("_check.php");
if ($forceLogin && !$uid) {
	require_once("login.php");
}

require_once("proc/addon.php");
require_once("proc/ai_vars.php");

$validThemes = ["green", "blue", "red"];
$theme = (isset($_COOKIE["theme"]) && in_array($_COOKIE["theme"], $validThemes))
    ? $_COOKIE["theme"]
    : "green";
