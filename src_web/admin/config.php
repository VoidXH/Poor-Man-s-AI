<?php
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
		<span class="input-group-text">'.$name.':</span>
		<input class="form-control"'.($int ? " type=\"number\"" : "").' name="value" value="'.htmlspecialchars($$var).'">
		<button type="submit" class="btn btn-primary">Set</button>
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
<h2>Branding</h2>
<?php
editVar("Website name", "siteName");
editVar("Chatbot page title", "chatPage");
editVar("Chatbot name", "chatName");
editVar("Image generator page title", "moaPage");
editVar("Image generator name", "moaName");
?>
<h2><br>Registration</h2>
<?php editBool("Open the site", "open"); ?>
<p>Allow anyone to use the website without logging in.</p>
<?php editBool("Force login", "forceLogin"); ?>
<p>Checking this will allow users to still use embedded chats, but not the site.</p>
<?php editBool("Allow registration", "regOn"); ?>
<h2><br>Behavior</h2>
<?php editVar("Chat models", "chatModels"); ?>
<p class="mt-n3">The available models by name, as set up on the Processors.</p>
<?php editBool("SLM warnings", "slmWarning"); ?>
<p>Show SLM warnings in chat when in Image + SLM mode. Recommended to disable when the Processor has unified memory and the models are large enough (like 7B).</p>
<h2><br>Limits</h2>
<?php editVar("Command lifespan", "commandClear", true); ?>
<p class="mt-n3">Unprocessed commands will be deleted from the database after this many minutes while the Processor is online.</p>
<?php editVar("Processor timeout", "procTimeout", true); ?>
<p class="mt-n3">Seconds to consider the processor offline after it was last seen.</p>
<?php editVar("Max queue length", "maxQueueLength"); ?>
<p class="mt-n3">Maximum number of prompts in queue. When the queue limit is reached, prompts will be canceled due to server overload.</p>
<?php editVar("Max wrong answers", "maxWrongAnswers"); ?>
<p class="mt-n3">Number of conversations with disliked responses to keep. Each dislike log is maximized at 64 kB, so the default value of 1000 limits dislike storage to 64 MB.</p>
<h2><br>For developers</h2>
<?php
editVar("Viewport", "viewport");
editVar("Bootstrap CSS path", "bootstrapPath");
editVar("Bootstrap JS path", "bootstrapJSPath");
editVar("JQuery path", "jqueryPath");
editVar("Marked path", "markedPath");
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    file_put_contents("__config.php", "<"."?php".PHP_EOL.$result."?".">");
}
?>