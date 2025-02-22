document.getElementById("image-form").addEventListener("keydown", function(event) {
  if (event.key === "Enter" && !document.getElementById("generate-btn").disabled) {
    generate();
    event.preventDefault();
  }
});

document.getElementById("num-images").addEventListener("input", function () {
  let value = parseInt(this.value);
  if (value < this.min) {
    this.value = this.min;
  } else if (value > this.max) {
    this.value = this.max;
  }
});

var debug = false;

var prompt;
var numImages;
var currentImage;

const displayImg = (result, prompt) => "<img src='data:image/png;base64," + result + "' title='" + prompt + "' class='img-fluid p-3' />";

function activate(running) {
  $("#generate-btn").prop("disabled", running);
  $("#help-btn").prop("disabled", running);
  $("#stop-btn").prop("disabled", !running);
}

function generateNextImage() {
  sendCommand("Image", prompt);
}

function updateProgressBar(progress) {
  const displayedProgress = Math.ceil((currentImage * 100 + progress) / numImages) + "%";
  if (progress > 0) {
    $("#progress-bar").css("width", displayedProgress).text(displayedProgress);
    $("#message").html("");
  }
}

function onPartialResult(progress, result) {
  if (progress < -1) {
    $("#message").html("Position in queue: " + (-1 - progress));
    return;
  }
  updateProgressBar(progress);
  if (result) {
    const div = $("#image-partial");
    const img = displayImg(result, prompt);
    if (debug) {
      div.prepend(img);
    } else {
      div.html(img);
    }
  }
}

function onFinalResult(progress, result) {
  updateProgressBar(progress);
  if (++currentImage == numImages) {
    activate(false);
  } else {
    generateNextImage();
  }
  $("#image-results").prepend(displayImg(result, prompt));
  if (!debug) {
    $("#image-partial").html("");
  }
}

function generate() {
  activate(true);
  $("#message").html("Connecting...");
  $("#progress-bar").css("width", "0%");
  prompt = $("#prompt").val();
  numImages = $("#num-images").val();
  currentImage = 0;
  generateNextImage();
}

function stop() {
  $("#stop-btn").prop("disabled", true);
  numImages = currentImage + 1;
  $.post("commands.php", { stop: workingCommandId });
}
