<?php
require("../_check.php");
require("../proc/ai_vars.php");

$time = time();
echo $time - getAIVar("llm-available") <= 10 || $time - getAIVar("moa-available") <= 10;
?>