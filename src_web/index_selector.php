<?php
$gpt_bgs = glob("img/gptbg*.jpg");
$gpt_bg = $gpt_bgs[array_rand($gpt_bgs)];
$diff_bgs = glob("img/diffbg*.jpg");
$diff_bg = $diff_bgs[array_rand($diff_bgs)];
?>
<style>
	.gpt { background-image: url('<?=$gpt_bg ?>'); }
	.moa { background-image: url('<?=$diff_bg ?>'); }
</style>
<div class="main">
	<div class="option gpt" onclick="gpt()"><span><?=$chatPage ?></span></div>
	<div class="option moa" onclick="moa()"><span><?=$moaPage ?></span></div>
</div>
