function modeSwap(mode) {
    sendCommand('Mode', mode);
    $('#loading-overlay').css('display', 'block');
}

function enableMoA() { modeSwap(5); }
function enableLLM() { modeSwap(2); }

function onPartialResult(progress, result) {}
function onFinalResult(progress, result) { window.location.reload(false); }