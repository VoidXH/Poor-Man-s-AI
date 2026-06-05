<p class="text-center small m-2"style="font-size: 0.7rem">
<?php if ($uid) { ?>
<?=$chatName ?> can make mistakes. Check important info.
<?php } else { ?>
By chatting with <?=$chatName ?>, you state that you have read the <a href="tos.php">Terms of Service</a> and the <a href="gdpr.php">Privacy Policy</a>, and agree to both.
<?php } ?>
</p>