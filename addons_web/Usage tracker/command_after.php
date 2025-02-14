<?php
$range = 900; // 15 minutes in seconds

$result = $sqlink->query("SELECT COUNT(*) FROM ai_commands WHERE progress != 100");
if ($result) {
  $row = $result->fetch_assoc();
  $usage = $row["COUNT(*)"];

  $time = time();
  $time = date("Y-m-d H:i:s", floor($time / $range) * $range);
  $result = $sqlink->query("SELECT * FROM ai_usage WHERE time = '$time'");
  if (mysqli_num_rows($result) > 0) {
    $row = $result->fetch_assoc();
    $oldUsage = $row["usage"];
    if ($usage > $oldUsage) {
      $sqlink->query("UPDATE ai_usage SET time = '$time', `usage` = $usage WHERE time = '$time'");
    }
  } else {
    $sqlink->query("INSERT INTO ai_usage (time, `usage`) VALUES ('$time', $usage)");
  }
}
?>
