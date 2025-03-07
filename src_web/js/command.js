// Poor Man's AI command API. Called functions must be implemented for a progress between 0-100 and the partial or full result:
// - onPartialResult(progress, result)
// - onFinalResult(progress, result)
// - onHTTPError(errorCode)
// If progress == -1: canceled command
// If progress < -1: queue pos = -1 - progress
// Also include JQuery.

var interval; // timer callback
var workingCommandId; // slow internet debounce
var fetching = false; // one poll at a time

function sendCommand(type, prompt) {
  $.post('commands.php', { command: type + '|' + prompt }, function(id) {
    workingCommandId = id;
    interval = setInterval(() => {
      if (!fetching) {
        fetching = true;
        $.get('commands.php?check=' + id, function(data) {
          fetching = false;
          progressCheck(id, data);
        })
        .fail((jqXHR, textStatus, errorThrown) => {
          fetching = false;
          onHTTPError(jqXHR.status);
        });
      }
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