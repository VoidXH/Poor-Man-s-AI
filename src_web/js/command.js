// VoidAI command API. Called functions must be implemented for a progress between 0-100 and the partial or full result:
// - onPartialResult(progress, result)
// - onFinalResult(progress, result)
// Also include JQuery.

var interval; // timer callback
var workingCommandId; // slow internet debounce

function sendCommand(type, prompt) {
  $.post('commands.php', { command: type + '|' + prompt }, function(id) {
    workingCommandId = id;
    interval = setInterval(() => {
      $.get('commands.php?check=' + id, function(data) {
        progressCheck(id, data);
      });
    }, 1500);
  });
}

function progressCheck(id, data) {
  if (id != workingCommandId) return;
  const split = data.indexOf('|');
  const progressValue = parseInt(data.substr(0, split));
  const result = data.substr(split + 1);
  if (progressValue >= 100) {
    clearInterval(interval);
    onFinalResult(progressValue, result);
    workingCommandId = -1;
  }
  else onPartialResult(progressValue, result);
}
