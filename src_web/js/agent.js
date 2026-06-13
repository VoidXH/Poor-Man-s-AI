const customPath = typeof pmaiPath !== "undefined" ? pmaiPath : "";

const escape = (x) => x.replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;").replaceAll('"', "&quot;").replaceAll("'", "&#039;");

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

function getPrompt() {
	const fileBlocks = [];
	$("#file-blocks-container .file-block").each(function() {
		fileBlocks.push("[File:" + $(this).attr("data-path") + "]");
	});
	return fileBlocks.join('') + $("#input").val();
}

function send() {
	if ($("#send").prop("disabled")) {
		return;
	}
	const input = getPrompt();
	if (input) {
		activate(true);
		$("#display").html('<div class="message"><p class="username">' + you + '</p><p class="text">' + escape(input) + '</p></div>' +
			'<div class="message reply"><p class="username">Agent</p><p id="result" class="text">...</p></div>');
		$("#input").val("");
		$("#file-blocks-container").empty();
		sendCommand("Agent", getPath() + "|" + input, customPath);
		$("#display").animate({ scrollTop: $("#display").prop("scrollHeight") }, 500);
	}
}

function reset() {
	$("#input").val("");
	$("#file-blocks-container").empty();
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

function sendCommandByPrompt(command) {
	$("#input").val("[" + command + "]");
	send();
}

function removeFileBlock(blockId) {
	$("#file-block-" + blockId).remove();
	reindexFileBlocks();
}

function reindexFileBlocks() {
	$("#file-blocks-container .file-block").each(function(index) {
		$(this).attr("data-index", index);
	});
}

function prependFileCommand(path) {
	const index = $("#file-blocks-container .file-block").length;
	const fileName = path.split(/[\\/]/).pop();
	const blockHtml = '<div class="file-block" id="file-block-' + index + '" data-path="' + escape(path) + '"><span class="file-block-name">' + escape(fileName) + '</span><button class="file-block-remove" onclick="removeFileBlock(' + index + ')" title="Remove">×</button></div>';
	$("#file-blocks-container").append(blockHtml);
	$("#input").focus();
}
