<?php
if (!$admin) {
    die;
}

require_once("proc/addon.php");

$description = [
    "agent-available" => "Last availability of a Processor with agent support (Unix timestamp)",
    "agent-weight" => "The highest priority among the agents",
    "llm-available" => "Last availability of a Processor with chat support (Unix timestamp)",
    "llm-weight" => "The highest priority among the chat Processors",
    "local-ip" => "Local IP of the Processor that has the <code>LocalIPLogger</code> extension enabled",
    "moa-available" => "Last availability of a Processor with image generation support (Unix timestamp)",
    "moa-weight" => "The highest priority among the image generation Processors",
    "mode" => "Engines to make available (<code>EngineCacheMode</code> enum in the Processor's code)",
    "unreg-use" => "Number of prompts sent by unregistered users",
];
?>
<h2>Var Dump</h2>
<table class="table table-striped table-dark">
    <thead>
      <tr>
        <th>Key</th>
        <th>Value</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
        <?php
            $result = $sqlink->query("SELECT * FROM ai_vars");
            while($row = $result->fetch_assoc()) {
                $key = htmlspecialchars($row["key"]);
                $value = htmlspecialchars($row["value"]);
                $desc = $description[$row["key"]] ?? "";
                echo "<tr><td>$key</td><td>$value</td><td>$desc</td></tr>";
            }
            addon("var_dump");
        ?>
    </tbody>
</table>