<?php
if (!$admin) {
    die;
}

$time = time();
$online = $time - getAIVar("llm-available") <= 10 || $time - getAIVar("moa-available") <= 10;
$mode = getAIVar("mode");
$moaClass = $mode == 5 ? "border-primary" : "";
$llmClass = $mode == 2 ? "border-primary" : "";
?>
<h2>Mode Selector</h2>
<?php if (!$online) { ?>
<div class="alert alert-warning mt-3">
    The Processor is currently <strong>offline</strong>. Mode switching is disabled until it comes back online.
</div>
<?php } ?>
<div class="row mt-3 g-3">
    <div class="col-md-6">
        <div class="card h-100 <?= $moaClass ?>">
            <div class="card-header">Image + SLM</div>
            <div class="card-body">
                <p class="card-text">Multimodal mode using a Small Language Model for chat and image generation capabilities.</p>
                <p class="mb-0"><strong>Mode ID:</strong> 5</p>
            </div>
            <div class="card-footer">
                <button class="btn btn-primary w-100" onclick="modeSwap(5)" <?= $online ? '' : 'disabled' ?>>Switch to Image + SLM</button>
            </div>
        </div>
    </div>
    <div class="col-md-6">
        <div class="card h-100 <?= $llmClass ?>">
            <div class="card-header">LLM</div>
            <div class="card-body">
                <p class="card-text">Pure language model mode for text-based chat only.</p>
                <p class="mb-0"><strong>Mode ID:</strong> 2</p>
            </div>
            <div class="card-footer">
                <button class="btn btn-primary w-100" onclick="modeSwap(2)" <?= $online ? '' : 'disabled' ?>>Switch to LLM</button>
            </div>
        </div>
    </div>
</div>
<div id="mode-status" class="mt-3"></div>
