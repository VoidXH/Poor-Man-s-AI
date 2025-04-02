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

$("#reference").on("change", function (event) {
  const fileName = event.target.files[0] ? event.target.files[0].name : "Choose file";
  $(this).next(".custom-file-label").text(fileName);
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
  if (progress == -1) {
    $("#message").html("MoA selection in progress...");
    return;
  } else if (progress < -1) {
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

function onHTTPError(errorCode) {
  if (errorCode == 503) {
    $("#message").html("Server is overloaded. Please try again later by pressing&nbsp;<i>Generate Image</i>&nbsp;again.");
    activate(false);
    return;
  }
  $("#message").html("Temporary error (HTTP " + errorCode + "), retrying...");
}

function clearRef() {
  $("#reference").val("");
  $("#ref-file-name").html("Choose file");
}

function generateFinish() {
  numImages = $("#num-images").val();
  currentImage = 0;
  generateNextImage();
}

function generate() {
  promptRead = $("#prompt").val();
  if (promptRead.length == 0) {
    $("#message").html("The prompt field is required.");
    return;
  }
  activate(true);
  $("#message").html("Connecting...");
  $("#progress-bar").css("width", "0%");
  prompt = promptRead;

  const refImage = $("#reference")[0];
  if (refImage.files.length == 0) {
    generateFinish();
    return;
  }

  const file = refImage.files[0];
  const reader = new FileReader();
  reader.onload = function (event) {
    const img = new Image();
    img.src = event.target.result;
    img.onload = function () {
      const canvas = $("<canvas>")[0];
      canvas.width = 128;
      canvas.height = 128;
      const ctx = canvas.getContext("2d");
      ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
      prompt = canvas.toDataURL("image/png") + "|" + prompt;
      $(canvas).remove();
      generateFinish();
    };
  };
  reader.readAsDataURL(file);
}

function stop() {
  $("#stop-btn").prop("disabled", true);
  numImages = currentImage + 1;
  $.post("commands.php", { stop: workingCommandId });
}
