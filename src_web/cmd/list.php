<?php // GET endpoint: list all unfinished jobs, optionally update Processor availability
require_once("availability.php");

if (!$admin) {
    die;
}

$result = $sqlink->query("SELECT id, command FROM ai_commands WHERE progress != 100 ORDER BY command_ts ASC");
$commands = [];
while ($row = mysqli_fetch_assoc($result)) {
    $command = $row["command"];
    $idx = strpos($command, '|');
    if ($idx === false) {
        continue;
    }

    $type = substr($command, 0, $idx);
    if ($type == "Chat") {
        $add = $llm;
    } else if ($type == "Image") {
        $add = $moa;
    } else {
        $add = true;
    }
    if ($add) {
        $commands[] = [
            "id" => $row["id"],
            "command" => $command
        ];
    }
}
echo json_encode(["commands" => $commands]);
?>