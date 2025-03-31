<?php
  // Name of the website:
  $siteName = "Poor Man's AI";
  // Title of the chatbot page:
  $chatPage = "Chat";
  // Name of the chatbot:
  $chatName = "Poor Man's GPT";
  // Title of the image generator page:
  $moaPage = "Make images";
  // Name of the image generator:
  $moaName = "VoidMoA";

  // Allow anyone to use your website without registration (true or false):
  $open = true;
  // Even if the site is open, require registration (true or false, checking
  // this will allow users to still use embedded chats, but not the site):
  $forceLogin = false;
  // Allow the registration of new users (true or false):
  $regOn = true;

  // The available models by name, as set up on the Processors:
  $chatModels = "Chat, Think, Code";
  // To disable showing "The quality and generation speed of answers will be
  // dramatically worse" warning in chats in SLM mode, because your VRAM is so
  // much that the reduced size models are still very large, set this to false:
  $slmWarning = true;
  // Minutes to delete unprocessed commands after:
  $commandClear = 3;
  // Seconds of no connection after the Processor is considered offline:
  $procTimeout = 15;
  // Number of conversations with disliked responses to keep (each one is
  // maximized at 64 kB, so the default value of 1000 limits dislike storage to
  // 64 MB):
  $maxWrongAnswers = 1000;

  // To enable zooming on phones, just remove the last parameter from this:
  $viewport = '<meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no, user-scalable=no">';

  // If you want to self-host JS dependencies too or the CDNs went down, change
  // these to the corresponding local relative paths:
  $bootstrapPath = "https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css";
  $bootstrapJSPath = "https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js";
  $jqueryPath = "https://code.jquery.com/jquery-3.6.0.min.js";
  $popperPath = "https://cdn.jsdelivr.net/npm/popper.js@1.12.9/dist/umd/popper.min.js";
  $markedPath = "https://cdn.jsdelivr.net/npm/marked/marked.min.js";
?>