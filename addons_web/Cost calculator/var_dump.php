<?php
// Configuration - modify these values
$wattage = 50; // Power draw under prompt processing - this is for a Mac mini M4 Pro
$promptTime = 13; // Average time spent generating a prompt, in seconds
$currency = 'HUF '; // Currency prefix
$wattCost = 45; // Price per kWh in your currency
// End of configuration --------------

$unreg = $sqlink->query('SELECT value FROM ai_vars WHERE `key` = "unreg-use"')->fetch_assoc()['value'] * $promptTime;
$unregCost = round($unreg / 3600 * $wattCost);
$reg = $sqlink->query('SELECT SUM(prompts) FROM ai_users')->fetch_row()[0] * $promptTime;
$regCost = round($reg / 3600 * $wattCost);
$total = $unreg + $reg;
$totalCost = $unregCost + $regCost;

function secondsToTime($seconds) {
	$days = (int)($seconds / 86400);
	$hours = (int)(($seconds % 86400) / 3600);
	$minutes = (int)(($seconds % 3600) / 60);
	$seconds %= 60;
	printf('%02d:%02d:%02d:%02d', $days, $hours, $minutes, $seconds);
}
?>
<tr><td>Reg. proc. time</td><td><?=secondsToTime($reg)?></td><td>Time spent processing the prompts of registered users.</td></tr>
<tr><td>Reg. cost</td><td><?=$currency.$regCost?></td><td>Registered users spent this much of your money.</td></tr>
<tr><td>Unreg. proc. time</td><td><?=secondsToTime($unreg)?></td><td>Time spent processing the prompts of unregistered users.</td></tr>
<tr><td>Unreg. cost</td><td><?=$currency.$unregCost?></td><td>Unregistered users spent this much of your money.</td></tr>
<tr><td>Total proc. time</td><td><?=secondsToTime($total)?></td><td>Time spent processing all prompts.</td></tr>
<tr><td>Total cost</td><td><?=$currency.$totalCost?></td><td>People spent this much of your money.</td></tr>
<tr><td>Total prompts</td><td><?=$total / $promptTime?></td><td>Total prompts processed across all users, including unregistereds.</td></tr>
<tr><td>Water boiled</td><td><?=round($total * 0.003)?> liters</td><td>Processing all the prompts boiled this much water for cooling.</td></tr>