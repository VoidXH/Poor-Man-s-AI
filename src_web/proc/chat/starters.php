<?php
require_once("__chat.php");
?>
<div class="message reply">
    <p class="text">Hi! You can ask <?=$chatName ?> anything with the chat window or try these conversation starters:<br>
    <?php foreach ($starters as $starter) {
        echo " <a class=\"starter-btn mt-2\" onclick=\"starter('{$starter[1]}')\">{$starter[0]}</a>";
    } ?></p>
</div>