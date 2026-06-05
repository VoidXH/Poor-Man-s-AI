<?php
require_once("_check.php");
if ($forceLogin && !$uid) {
	require_once("login.php");
}

require_once("proc/addon.php");
require_once("proc/ai_vars.php");
