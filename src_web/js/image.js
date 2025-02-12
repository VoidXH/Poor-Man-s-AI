document.getElementById('image-form').addEventListener('keydown', function(event) {
  if (event.key === 'Enter' && !document.getElementById('generate-btn').disabled) {
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

var prompt;
var numImages;
var currentImage;

function activate(running) {
  $('#generate-btn').prop('disabled', running);
  $('#help-btn').prop('disabled', running);
  $('#stop-btn').prop('disabled', !running);
}

function generateNextImage() {
  sendCommand('Image', prompt);
}

function updateProgressBar(progress) {
  const displayedProgress = Math.ceil((currentImage * 100 + progress) / numImages) + '%';
  if (progress > 0) {
    $('#progress-bar').css('width', displayedProgress).text(displayedProgress);
    $('#message').html('');
  }
}

function onPartialResult(progress, result) {
  updateProgressBar(progress);
  if (result) $('#image-partial').html('<img src="data:image/png;base64,' + result + '" class="img-fluid mt-3" />');
}

function onFinalResult(progress, result) {
  updateProgressBar(progress);
  if (++currentImage == numImages) {
    activate(false);
  } else {
    generateNextImage();
  }
  $('#image-results').prepend('<img src="data:image/png;base64,' + result + '" class="img-fluid p-3" />');
  $('#image-partial').html('');
}

function generate() {
  activate(true);
  $('#message').html('MoA selection in progress...');
  $('#progress-bar').css('width', '0%');
  prompt = $('#prompt').val();
  numImages = $('#num-images').val();
  currentImage = 0;
  generateNextImage();
}

function stop() {
  $('#stop-btn').prop('disabled', true);
  numImages = currentImage + 1;
  $.post('commands.php', { stop: workingCommandId });
}
