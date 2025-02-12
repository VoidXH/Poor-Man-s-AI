<?php
require("__config.php");
?>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title><?=$siteName ?> Terms of Service</title>
  <link rel="stylesheet" href="<?=$bootstrapPath; ?>">
  <link rel="stylesheet" href="css/dark.css">
</head>
<body>
<div class="container">
  <div class="text-center mb-3 pt-3">
    <h3 class="font-weight-light">Terms of Service</h3>
  </div>
  <p>By using this service, you accept that:</p>
  <ul>
    <li>The online status of the service is never guaranteed.</li>
    <li>The quality of results is not guaranteed by any metric.</li>
    <li>Content can be generated in infrequent chunks, and this is by design.</li>
    <li>Chat answers can be incorrect, even when asked about important info.</li>
    <li>You have no guaranteed place in any queue.</li>
    <li>Your commands can be canceled at any time for any reason.</li>
    <li>Erotic imagery can accidentally be generated, even though there are safeguards against it. You agree you will not try to bypass these safeguards.</li>
    <li>If you register, your data is handled according to the <a href="gdpr.php">Privacy Policy</a>.</li>
  </ul>
  <a class="btn btn-success mb-3" onclick="history.back(-1);">Back</a>
</div>
</body>
</html>
