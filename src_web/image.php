<?php
require_once("_check.php");
if ($forceLogin && !$uid) {
    require_once("login.php");
}

require_once("proc/addon.php");
require_once("proc/ai_vars.php");
$offline = time() - getAIVar("moa-available") > $procTimeout;
?>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title><?=$moaName ?></title>
  <link rel="stylesheet" href="<?=$bootstrapPath ?>">
  <link rel="stylesheet" href="css/moa.css">
  <link rel="stylesheet" href="css/dark.css">
</head>
<body>
<div class="container">
  <a href="index.php" class="btn btn-primary back mt-3">Back</a>
  <div class="text-center mb-3 pt-3">
    <img src="img/moa_banner.png">
    <h5 class="font-weight-light">Mixture-of-Artists image generator</h5>
  </div>
<?php
  if ($offline) {
?>
  <div class="alert alert-danger" role="alert">
    Shhh! The image generating computer is sleeping and can't work now. If the home page said it's online, it might be in <i>chat-only</i> mode and the admin could switch it back to <i>image + chat</i>. But reading is available until it comes back: <a class="btn btn-success" href="image_help.php">Help</a>
  </div>
<?php } else {
  addon("image");
?>
  <form id="image-form" class="mb-3" autocomplete="off">
    <div class="mb-3">
      <input type="text" class="form-control" id="prompt" name="prompt" placeholder="Enter prompt..." autofocus required>
    </div>
    <div class="mb-3 input-group">
      <div class="input-group-prepend">
        <span class="input-group-text">Number of images:</span>
      </div>
      <input type="number" class="form-control" id="num-images" name="num-images" min="1" max="10" value="1" required>
    </div>
    <div class="d-flex justify-content-center">
      <button type="button" class="btn btn-primary" id="generate-btn" onclick="generate()">Generate Image</button>
      <button type="button" class="btn btn-danger ml-2 mr-2" id="stop-btn" onclick="stop()" disabled>Stop</button>
      <a class="btn btn-success" href="image_help.php">Help</a>
    </div>
    <?php if (!$uid) { ?>
    <p class="text-center mt-3"><small>By using <?=$moaName ?>, you state that you have read the <a href="tos.php">Terms of Service</a> and the <a href="gdpr.php">Privacy Policy</a>, and agree to both.</small></p>
    <?php } ?>
  </form>
  <div class="progress mb-3" style="height: 30px; position: relative;">
    <div id="progress-bar" class="progress-bar" role="progressbar" style="width: 0%;">0%</div>
    <div id="message" class="text-center" style="position: absolute; top: 0; left: 0; right: 0; bottom: 0; display: flex; align-items: center; justify-content: center; color: black;"> </div>
  </div>
  <div id="image-partial" class="text-center"></div>
  <div id="image-results" class="text-center"></div>
<?php } ?>
</div>
<a class="br" href="https://github.com/VoidXH/Poor-Man-s-AI"><img src="img/github.svg"></a>
<script src="<?=$jqueryPath ?>"></script>
<script src="js/command.js"></script>
<script src="js/image.js"></script>
</body>
</html>