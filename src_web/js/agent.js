const customPath = typeof pmaiPath !== "undefined" ? pmaiPath : "";

const escape = (x) => x.replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;").replaceAll('"', "&quot;").replaceAll("'", "&#039;");

$(document).ready(function () {
	var firstItem = $("#pathMenu a:first");
	if (firstItem.length > 0) {
		firstItem.addClass("active");
		$("#pathLabel").text(firstItem.text());
	}

	var firstAgent = $("#agentMenu a:first");
	if (firstAgent.length > 0) {
		firstAgent.addClass("active");
		$("#agentLabel").text(firstAgent.text());
	}

	$("#chat, #think, #code").click(function () {
		$("#chat, #think, #code").removeClass("btn-primary").addClass("btn-secondary");
		$(this).removeClass("btn-secondary").addClass("btn-primary");
	});

	$("#pathMenu a").click(function (e) {
		e.preventDefault();
		$("#pathMenu a").removeClass("active");
		$(this).addClass("active");
		$("#pathLabel").text($(this).text());
		$('#pathDropdownBtn').dropdown('toggle');
	});

	$("#agentMenu a").click(function (e) {
		e.preventDefault();
		$("#agentMenu a").removeClass("active");
		$(this).addClass("active");
		$("#agentLabel").text($(this).text());
		$('#agentDropdownBtn').dropdown('toggle');
	});
});

function getPath() {
	var activeItem = $("#pathMenu a.active");
	if (activeItem.length > 0) {
		return activeItem.attr("data-path");
	}
	return "";
}

function getAgent() {
	var activeItem = $("#agentMenu a.active");
	if (activeItem.length > 0) {
		return activeItem.attr("data-agent");
	}
	return "";
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

	$("#result").html(marked.parse(result, { breaks: true }));
	$("#result pre code").each(function (i, block) {
		hljs.highlightElement(block);
	});
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
	$("#file-blocks-container .file-block").each(function () {
		fileBlocks.push("[File:" + $(this).attr("data-path") + "]");
	});
	return fileBlocks.join('') + $("#input").val();
}

function send(forceQueue) {
	if ($("#send").prop("disabled")) {
		return;
	}
	const inputRaw = $("#input").val().trim();
	const input = getPrompt();
	if (input) {
		activate(true);
		$("#display").html('<div class="message"><p class="username">' + you + '</p><p class="text">' + escape(input) + '</p></div>' +
			'<div class="message reply"><p class="username">Agent</p><p id="result" class="text">...</p></div>');
		$("#input").val("");
		$("#file-blocks-container").empty();
		const prompt = input;
		const agent = getAgent();
		const agentPrefix = agent ? "<" + agent + ">" : "";
		const fullCommand = agentPrefix + getPath() + "|" + prompt;
		if (forceQueue === true || (forceQueue !== false && agentQueueByDefault && !inputRaw.startsWith('['))) {
			sendCommand("Agent", "[Queue:" + fullCommand + "]", customPath);
		} else {
			sendCommand("Agent", fullCommand, customPath);
		}
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

$("#input").keypress(function (e) {
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
	$("#file-blocks-container .file-block").each(function (index) {
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
