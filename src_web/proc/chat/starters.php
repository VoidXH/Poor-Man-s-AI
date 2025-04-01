<?php
require_once("__chat.php");
?>
<div class="message reply">
    <p class="text">Hi! You can ask <?=$chatName ?> anything with the chat window or try these conversation starters:<br>
    <?php foreach ($starters as $starter) {
        echo " <a class=\"btn btn-secondary btn-sm mt-2 text\" onclick=\"starter('{$starter[1]}')\">{$starter[0]}</a>";
    } ?></p>
</div>