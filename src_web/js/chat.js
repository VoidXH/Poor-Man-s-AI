var sentMessages = 0;
var newMessage = false;
var hist = Array();

const escape = (x) => x.replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;").replaceAll('"', "&quot;").replaceAll("'", "&#039;").replaceAll("\n", "<br>");

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
  console.log(result);
  const split = splitThink(result);
  let think = "";
  if (split.think.length != 0) {
    const prevThink = $('#think' + sentMessages).hasClass('show');
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
  hist.push(result);
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
    if (hist.length > 8) {
        hist = hist.slice(-8);
    }
    sentMessages++;
    newMessage = true;
    hist.push(input);
    sendCommand("Chat", getModel() + "|" + hist.map(str => str.replaceAll("|", "&vert;")).join("|"));
    $(".chatbox").append("<div class='message'><span class='username'>" + you + "</span><span class='text'>" + escape(input) + "</span></div>");
    $(".chatbox").append("<div class='message reply'><span class='username'>" + gpt + "</span><span id='msg" + sentMessages + "' class='text'>...</span></div>");
    $("#input").val("");
    $(".chatbox").animate({ scrollTop: $(".chatbox").prop("scrollHeight") }, 500);
  }
}

function stop() {
  $("#stop").prop("disabled", true);
  $.post("commands.php", { stop: workingCommandId });
}

$("#input").keypress(function(e) {
  if (e.key === "Enter" && !e.shiftKey) {
    e.preventDefault();
    $("#send").click();
  }
});
