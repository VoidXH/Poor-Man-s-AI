function onPartialResult(progress, result) {
	if (progress < -1) {
		$("#output").text("Position in queue: " + (-1 - progress) + "\n");
		return;
	}
	$("#output").text(result);
}

function onFinalResult(progress, result) {
	onPartialResult(progress, result);
	$("#output").text($("#output").text() + "\n");
	$("#cmd").prop("disabled", false);
	$("#sendBtn").prop("disabled", false);
	$("#cmd").focus();
}

function onHTTPError(errorCode) {
	if (errorCode == 503) {
		onFinalResult(100, "Server is overloaded. Please try again later.");
		return;
	}
	onPartialResult(50, "Temporary error (HTTP " + errorCode + "), retrying...");
}

$(document).ready(function() {
	$("#sendBtn").click(function() {
		sendShellCommand();
	});

	$("#cmd").keypress(function(e) {
		if (e.key === "Enter" && !e.shiftKey) {
			e.preventDefault();
			sendShellCommand();
		}
	});
});

function sendShellCommand() {
	var cmd = $("#cmd").val().trim();
	if (!cmd) return;

	$("#cmd").prop("disabled", true);
	$("#sendBtn").prop("disabled", true);

	var current = $("#output").text();
	$("#output").text(current + promptUser + "@" + siteName + ":~$ " + cmd + "\n");

	sendCommand("Shell", cmd);
	$("#cmd").val("");
}
