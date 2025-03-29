<?php
require("__config.php");
require("_check.php");
if (!$admin) {
    die;
}

$result = "";

function editVar($name, $var, $int = false) {
    global $$var, $result;
    if ($_POST["key"] == $var) { $$var = $int ? intval($_POST["value"]) : $_POST["value"]; }
    $result .= "$".$var."= ".var_export($$var, true).";".PHP_EOL;
        echo '<form method="POST">
    <div class="input-group mt-3">
        <div class="input-group-prepend">
            <span class="input-group-text">'.$name.':</span>
        </div>
        <input class="form-control"'.($int ? " type=\"number\"" : "").' name="value" value="'.htmlspecialchars($$var).'">
        <div class="input-group-append">
            <button type="submit" class="btn btn-primary">Set</button>
        </div>
    </div>
    <input type="hidden" name="key" value="'.$var.'">
</form>';
}

function editBool($name, $var) {
    global $$var, $result;
    if ($_POST["key"] == $var) { $$var = $_POST["value"] == "on"; }
    $result .= "$".$var."= ".var_export($$var, true).";".PHP_EOL;
        echo '<form method="POST" class="mb-0">
    <div class="form-check form-switch">
        <input class="form-check-input" type="checkbox" name="value"'.($$var ? " checked" : "").'>
        <label class="form-check-label" for="value">'.$name.'</label>
        <button type="submit" class="btn btn-primary btn-sm">Set</button>
    </div>
    <input type="hidden" name="key" value="'.$var.'">
</form>';
}
?>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <?=$viewport ?>
    <title><?=$siteName ?> admin</title>
    <link rel="stylesheet" href="<?=$bootstrapPath ?>">
    <link rel="stylesheet" href="css/dark.css">
</head>
<body>
<div class="container">
    <a class="btn btn-primary mt-3" href="index.php">Back</a>
    <h1>Website configuration</h1>
    <h2>Branding</h2>
<?php
    editVar("Website name", "siteName");
    editVar("Chatbot page title", "chatPage");
    editVar("Chatbot name", "chatName");
    editVar("Image generator page title", "moaPage");
    editVar("Image generator name", "moaName");
?>
    <h2>Registration</h2>
    <?php editBool("Open the site", "open"); ?>
    <p>Allow anyone to use the website without logging in.</p>
    <?php editBool("Force login", "forceLogin"); ?>
    <p>Checking this will allow users to still use embedded chats, but not the site.</p>
    <?php editBool("Allow registration", "regOn"); ?>
    <h2>Behavior</h2>
    <?php editVar("Chat models", "chatModels"); ?>
    <p>The available models by name, as set up on the Processors.</p>
    <?php editBool("SLM warnings", "slmWarning"); ?>
    <p>Show SLM warnings in chat when in Image + SLM mode. Recommended to disable when the Processor has unified memory and the models are large enough (like 7B).</p>
<?php
    editVar("Minutes to delete unprocessed commands after", "commandClear", true);
    editVar("Seconds the processor is offline after", "procTimeout", true);
?>
    <h1>For developers</h1>
<?php
    editVar("Viewport", "viewport");
    editVar("Bootstrap CSS path", "bootstrapPath");
    editVar("Bootstrap JS path", "bootstrapJSPath");
    editVar("JQuery path", "jqueryPath");
    editVar("Popper path", "popperPath");
    editVar("Marked path", "markedPath");
    if ($_SERVER["REQUEST_METHOD"] == "POST") {
        file_put_contents("__config.php", "<"."?php".PHP_EOL.$result."?".">");
    }
?>
    <h2>Var dump</h2>
    <table class="table table-striped table-dark">
        <thead>
          <tr>
            <th>Key</th>
            <th>Value</th>
          </tr>
        </thead>
        <tbody>
            <?php
                $result = $sqlink->query("SELECT * FROM ai_vars");
                while($row = $result->fetch_assoc()) {
                    echo "<tr><td>".$row['key']."</td><td>".$row['value']."</td></tr>";
                }
            ?>
        </tbody>
    </table>
</div>
</body>
</html>