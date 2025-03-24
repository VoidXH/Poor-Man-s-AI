<?php // POST endpoint: update job progress: "id" -> job ID, "result" -> new status text (or base64 binary), "progress" -> new progress (1-100)
require_once("availability.php");

if ($admin && isset($_POST["id"])) {
  $id = intval($_POST["id"]);
  $newProgress = intval($_POST["progress"]);
  $result = $_POST["result"];
  $stmt = execute("SELECT progress FROM ai_commands WHERE id = ?", $id);
  $stmt->bind_result($progress);
  $stmt->fetch();
  $stmt->close();
  if ($newProgress == 100) {
    if ($result == null) {
      echo "RETRY Final result is null.";
      die;
    } else if (empty($result)) {
      echo "RETRY Final result is empty.";
      die;
    }
  } else if ($progress == -1) {
    echo "STOP";
    die;
  }

  $stmt = execute("UPDATE ai_commands SET result = ?, progress = ?, result_ts = NOW() WHERE id = ?", $result, $newProgress, $id);
  $stmt->close();
}
?>