<?php
$items = explode("|", $_POST["fill"]);
foreach ($items as $index => $text) {
    $text = nl2br(htmlspecialchars($text));
    echo '<div class="message';
    if ($index % 2 != 0) {
        echo " reply";
    }
    echo "\"><p class=\"text\">$text</p></div>";
}
?>