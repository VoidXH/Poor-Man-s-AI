<?php
require("__config.php");
?>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title><?=$siteName ?> Privacy Policy</title>
  <link rel="stylesheet" href="<?=$bootstrapPath; ?>">
  <link rel="stylesheet" href="css/dark.css">
</head>
<body>
<div class="container">
  <div class="text-center mb-3 pt-3">
    <h3 class="font-weight-light">Privacy Policy and Cookie Notice</h3>
  </div>
  <p>Our commitment to protecting your privacy is paramount. We handle your data with care, ensuring that your information is secure and used solely for the purpose of providing our services. This policy outlines the types of data we collect and manage, along with measures to safeguard your information.</p>
  <h4>Data We Collect</h4>
  <p>If you register, the following data will be stored and associated with your profile:</p>
  <ul>
    <li><b>Username:</b> We collect your username to identify your account and provide access to our services.</li>
    <li><b>Password:</b> Your password is securely stored using industry-standard cryptographic hashing techniques, designed to protect against unauthorized access, even in the case of a data breach. We recommend using a strong password of at least 12 characters to enhance security.</li>
    <li><b>Number of Prompts Sent:</b> We track the number of prompts you send to our AI models to improve our services and ensure optimal performance. This count is only a single number, is not linked to any personally identifiable information, and is used only internally.</li>
  </ul>
  <p>Your prompts are stored in the database only temporarily, unless you press the <i>mark answer as wrong</i> button, displayed as a thumbs down emoji. The lifecycle of your prompts is stated under <i>Handling of Prompts</i>, and is not affected by using the page with a registration or not.</p>
  <h4>Data Retention</h4>
  <p>We retain your data only as long as necessary to provide our services effectively. Once you delete your account, every single value described under <i>Data We Collect</i> is instantly removed from our database.</p>
  <h4>Data Sharing</h4>
  <p>Your data is never shared with third parties unless compelled to do so by law. We take these obligations seriously and only share information when legally mandated.</p>
  <h4>Cookie Policy</h4>
  <p>Cookies are small text files used by websites to remember information about users for purposes like logging in. On this site, persistent cookies are used solely to keep you logged in after you authenticate.
  Once logged in, cookies store your username and your securely salted password. These cookies are only present in your browser when you're actively logged in.
  When you log out, these cookies are automatically removed, ensuring that your data isn't accessible to others who might find the cookies after you've logged out.
  We do not use cookies for tracking user behavior or any other purposes beyond supporting the login process.
  Consent for third-party cookies is managed through their respective consent forms in case such additional software is used. For more information on third-party cookies used on this website, please review the relevant third-party privacy policies displayed on their consent forms.</p>
  <h4>Handling of Prompts</h4>
  <p>When you submit a prompt to our system, it is stored temporarily on the server while it is being processed. This storage includes any partial results generated during the processing.
  Additionally, once the corresponding AI has finished processing a prompt, and the result is sent to you, the content and any associated results are instantly removed from the server.
  Furthermore, if a prompt is stored on the server, and you didn't receive your result (because the browser was closed, network error, etc.), it will automatically be deleted after <?=$commandClear ?> minutes if the index page you see after logging in displays the status "Online".
  Importantly, these prompts are never linked to your user account, ensuring that your personal data remains protected.
  Any additional information stored alongside the prompts, such as the time the prompt was sent, whether it is a partial or final result, the last update to the result, and the progress percentage, is also removed once the final result is delivered to you.
  Deleting your account does not speed up this behavior: if you delete your account while a prompt is being processed, <?=$commandClear ?> minutes must pass while the server is online for the remaining prompts to be deleted.</p>
  <p>Pressing the <i>mark answer as wrong</i> button, which is displayed as a thumbs down emoji, will store the entire displayed chat in the database for manual review. The conversation might be used for checking factual errors, and improving the service to provide a correct response next time.
  If the report is considered false or can't be addressed for any reason, it will be discared without any action being taken. Deleting these chats after they have been addressed is the responsibility of the administrator.</p>
  <a class="btn btn-success mb-3" onclick="history.back(-1);">Back</a>
</div>
</body>
</html>
