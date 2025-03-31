var sentMessages = 0;
var newMessage = false;
var hist = Array();

const customPath = typeof pmaiPath !== "undefined" ? pmaiPath : "";

const escape = (x) => x.replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;").replaceAll('"', "&quot;").replaceAll("'", "&#039;").replaceAll("\n", "<br>");
const isWorking = () => $("#send").prop("disabled");
const warnWorking = () => alert("This option is only available when " + gpt + "'s answering is not in progress.");

$(document).ready(function() {
  $("#chat, #think, #code").click(function() {
    $("#chat, #think, #code").removeClass("btn-primary").addClass("btn-secondary");
    $(this).removeClass("btn-secondary").addClass("btn-primary");
  });
});

function reset() {
  $("#input").val("");
  $(".chatbox").html("");
  hist = Array();
}

function activate(running) {
  $("#reset").prop("disabled", running);
  $("#input").prop("disabled", running);
  $("#send").prop("disabled", running);
  const stop = $("#stop");
  stop.prop("disabled", false);
  if (running) {
    stop.show();
  } else {
    stop.hide();
  }
}

function splitThink(input) {
  let think = "";
  let result = "";
  const thinkStart = "<think>";
  const thinkEnd = "</think>";
  const startIdx = input.indexOf(thinkStart);
  const endIdx = input.indexOf(thinkEnd);
  if (startIdx != -1 && endIdx != -1) {
    think = input.substring(startIdx + thinkStart.length, endIdx).trim();
    result = input.substring(endIdx + thinkEnd.length).trim();
  } else if (startIdx != -1) {
    think = input.substring(startIdx + thinkStart.length).trim();
  } else {
    result = input;
  }
  return { think, result };
}

function onPartialResult(progress, result) {
  if (progress < -1) {
    $("#msg" + sentMessages).html("Position in queue: " + (-1 - progress));
    return;
  } else if (progress == 0 || result.length == 0) {
    return;
  }
  const split = splitThink(result.replaceAll("????", ""));
  let think = "";
  if (split.think.length != 0) {
    const prevThink = $("#think" + sentMessages).hasClass("show");
    think = '<button class="btn btn-secondary" type="button" data-toggle="collapse" data-target="#think' + sentMessages + '" aria-expanded="' + prevThink + '" aria-controls="think' + sentMessages + '">Show/hide thought process</button>' +
      '<div class="collapse' + (prevThink ? ' show' : '') + '" id="think' + sentMessages + '" style="border: 1px solid white; width: auto;">' + marked.parse(split.think) + '</div>';
  }
  $("#msg" + sentMessages).html(think + marked.parse(split.result));
  if (newMessage) {
    $(".chatbox").animate({ scrollTop: $(".chatbox").prop("scrollHeight") }, 500);
    newMessage = false;
  }
}

function onFinalResult(progress, result) {
  onPartialResult(progress, result);
  activate(false);
  if (!(/Mobi|Android/i.test(navigator.userAgent))) {
    $('#input').focus();
  }
  hist.push(result);
}

function onHTTPError(errorCode) {
  onPartialResult(50, "Temporary error (HTTP " + errorCode + "), retrying...");
}

function getModel() {
  var res;
  $("#model button").each(function() {
    if ($(this).attr("class").includes("btn-primary")) {
      res = $(this).text();
    }
  });
  return res;
}

function send() {
  const input = $("#input").val();
  if (input) {
    activate(true);
    if (sentMessages == 0) {
      reset();
    }
    sentMessages++;
    newMessage = true;
    hist.push(input);
    const toSend = hist.length > 8 ? hist.slice(-8) : hist;
    sendCommand("Chat", getModel() + "|" + toSend.map(str => str.replaceAll("|", "&vert;")).join("|"), customPath);
    $(".chatbox").append('<div id="out' + sentMessages + '" class="message"><p class="username">' + you + '</p><p class="text">' + escape(input) + '</p><button class="btn btn-secondary btn-sm option" onclick="edit(' + sentMessages + ')">&#9999; Edit</button></div>');
    $(".chatbox").append('<div id="in' + sentMessages + '" class="message reply"><p class="username">' + gpt + '</p><p id="msg' + sentMessages + '" class="text">...</p><div class="option">' +
        '<button class="btn btn-secondary btn-sm" onclick="regenerate(' + sentMessages + ')">&#128260; Regenerate</button>' +
        '<button class="btn btn-secondary btn-sm ml-2" id="correct' + sentMessages + '" onclick="correct(' + sentMessages + ')">&#128077;</button>' +
        '<button class="btn btn-secondary btn-sm ml-2" id="wrong' + sentMessages + '" onclick="wrong(' + sentMessages + ')">&#128078;</button>' +
        '</div></div>');
    $("#input").val("");
    $(".chatbox").animate({ scrollTop: $(".chatbox").prop("scrollHeight") }, 500);
  }
}

function clearChat(from) {
  for (let i = from + 1, sents = hist.length / 2; i <= sents; i++) {
    $("#out" + i).remove();
    $("#in" + i).remove();
  }
  hist = hist.slice(0, 2 * from);
  sentMessages = from;
}

function edit(id) {
  if (isWorking()) {
    warnWorking();
    return;
  }
  const userMessage = hist[2 * (id - 1)];
  clearChat(id - 1);
  $("#input").val(userMessage);
}

function regenerate(id) {
  edit(id);
  if (isWorking()) return;
  send();
}

function correct(id) {
  if (isWorking()) {
    warnWorking();
    return;
  }
  $("#correct" + id).remove();
  $("#wrong" + id).remove();
}

function wrong(id) {
  correct(id);
  if (isWorking()) return;
  const message = hist.join('|');
  const limited = message.length > 65536 ? message.slice(-65536) : message;
  $.post(customPath + "cmd/dislike.php", { history: limited });
}

function starter(prompt) {
  $("#input").val(prompt);
  send();
}

function stop() {
  $("#stop").prop("disabled", true);
  $.post(customPath + "commands.php", { stop: workingCommandId });
}

$("#input").keypress(function(e) {
  if (e.key === "Enter" && !e.shiftKey) {
    e.preventDefault();
    $("#send").click();
  }
});