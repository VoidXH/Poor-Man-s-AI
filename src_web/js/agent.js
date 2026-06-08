const customPath = typeof pmaiPath !== "undefined" ? pmaiPath : "";

const escape = (x) => x.replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;").replaceAll('"', "&quot;").replaceAll("'", "&#039;").replaceAll("\n", "<br>");

$(document).ready(function() {
	$("#chat, #think, #code").click(function() {
		$("#chat, #think, #code").removeClass("btn-primary").addClass("btn-secondary");
		$(this).removeClass("btn-secondary").addClass("btn-primary");
	});

	$("#path button").click(function() {
		$("#path button").removeClass("btn-primary").addClass("btn-secondary");
		$(this).removeClass("btn-secondary").addClass("btn-primary");
	});
});

function getPath() {
	var res = "";
	$("#path button").each(function() {
		if ($(this).attr("class").includes("btn-primary")) {
			res = $(this).text();
		}
	});
	return res;
}

function activate(running) {
	$("#send").prop("disabled", running);
	const stop = $("#stop");
	stop.prop("disabled", false);
	if (running) {
		stop.show();
	} else {
		stop.hide();
	}
}

function onPartialResult(progress, result) {
	if (progress < -1) {
		$("#display").html('<div class="alert alert-info" role="alert">Position in queue: ' + (-1 - progress) + '</div>');
		return;
	} else if (progress == 0 || result.length == 0) {
		return;
	}

	const decoder = new TextDecoder('utf-8');
	const encoder = new TextEncoder();
	const rawBytes = Uint8Array.from([...result].map(char => char.charCodeAt(0)));
	result = decoder.decode(rawBytes);
	$("#result").html(marked.parse(result));
}

function onFinalResult(progress, result) {
	onPartialResult(progress, result);
	activate(false);
}

function onHTTPError(errorCode) {
	if (errorCode == 503) {
		onFinalResult(100, "Server is overloaded. Please try again later.");
		return;
	}
	onPartialResult(50, "Temporary error (HTTP " + errorCode + "), retrying...");
}

function send() {
	if ($("#send").prop("disabled")) {
		return;
	}
	const input = $("#input").val();
	if (input) {
		activate(true);
		$("#display").html('<div class="message"><p class="username">' + you + '</p><p class="text">' + escape(input) + '</p></div>' +
			'<div class="message reply"><p class="username">Agent</p><p id="result" class="text">...</p></div>');
		$("#input").val("");
		sendCommand("Agent", getPath() + "|" + input, customPath);
		$("#display").animate({ scrollTop: $("#display").prop("scrollHeight") }, 500);
	}
}

function reset() {
	$("#input").val("");
	$("#display").html("");
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
