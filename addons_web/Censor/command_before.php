<?php
if (substr($_POST["command"], 0, 5) == "Image") {
// Alphabetical list of censored word beginnings (like erotic will also censor erotica)
  $keywords = [
"erotic",
"nsfw",
  ];

// List of censored phrases, can be contained anywhere in the prompt
  $selectors = [
"adult pic",
"not safe for work",
  ];

  function binarySearch(Array $arr, $start, $end, $x) { 
    if ($end < $start) {
      return false;
    }
    $mid = floor(($end + $start) / 2);
    if ($arr[$mid] == $x) {
      return true;
    } else if ($start == $end) {
      return strpos($x, $arr[$mid]) === 0 || strpos($x, $arr[$mid - 1]) === 0;
    } else if ($arr[$mid] > $x) {
      return binarySearch($arr, $start, $mid - 1, $x);
    } else {
      return binarySearch($arr, $mid + 1, $end, $x);
    }
  }

  $keywordc = count($keywords) - 1;
  $lower = strtolower(substr($_POST["command"], 6));
  $prepared = str_replace([',', '.', '-', '_', '=', '!', ';', '/', '|', '\\', '(', ')'], ' ', $lower);
  $promptKeys = explode(' ', $prepared);
  for ($i = 0; $i < count($promptKeys); $i++) {
    if (binarySearch($keywords, 0, $keywordc, $promptKeys[$i])) {
      die("-1");
    }
  }
  for ($i = 0; $i < count($selectors); $i++) {
    if (strpos($lower, $selectors[$i]) !== false) {
      die("-1");
    }
  }
}
?>
