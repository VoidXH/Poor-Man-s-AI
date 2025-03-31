<?php
if (!$admin) {
    die;
}

$description = [
    "llm-available" => "Last availability of a Processor with chat support (Unix timestamp)",
    "llm-weight" => "The highest priority among the chat Processors",
    "local-ip" => "Local IP of the Processor that has the <code>LocalIPLogger</code> extension enabled",
    "moa-available" => "Last availability of a Processor with image generation support (Unix timestamp)",
    "moa-weight" => "The highest priority among the image generation Processors",
    "mode" => "Engines to make available (<code>EngineCacheMode</code> enum in the Processor's code)",
    "unreg-use" => "Number of prompts sent by unregistered users",
];
?>
<h2>Var dump</h2>
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
                echo "<tr><td>".$row["key"]."</td><td>".$row["value"]."</td><td>".$description[$row["key"]]."</td></tr>";
            }
        ?>
    </tbody>
</table>