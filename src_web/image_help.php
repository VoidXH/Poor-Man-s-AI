<?php
require("__config.php");
?>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <?=$viewport ?>
  <title><?=$moaName ?></title>
  <link rel="stylesheet" href="<?=$bootstrapPath; ?>">
  <link rel="stylesheet" href="css/dark.css">
  <style> img { max-width: 100%; } li { padding-bottom: 6px; } </style>
</head>
<body>
<div class="container">
  <div class="text-center mb-3 pt-3">
    <img src="img/moa_banner.png">
    <h5 class="font-weight-light">prompt engineering basics</h5>
  </div>
  <p>Knowing the inner workings of VoidMoA will help you make better prompts. The basic idea behind VoidMoA is the Mixture-of-Artists model, first introduced here, resulting in very low (&lt;6 GB) VRAM usage while prompts can target any downloaded model.
  Before every image generation, the prompt is evaluated for the major image properties such as:</p>
  <ul>
    <li><b>Artist:</b> A very small and thus fast model that can be run even on a cheap gaming laptop, but is completely specialized to the general idea of what you want to generate. You might find a specific model is used with a very high chance for some keywords,
    for example, using the word &quot;photo&quot; in the prompt will most likely use a model that was only trained on photos and has no chance of generating stylized images.</li>
    <li><b>Negative prompt:</b> VoidMoA doesn't support explicit negative prompting like many other image generators, because negative prompts were preselected for each artist. Sorry, you can't define what not to have on your images.
    Just generate 10 and some might not have what you don't want, this thing is fast and practically free.</li>
    <li><b>Image size:</b> The resolution and aspect ratio is affected by prompting. Try &quot;horizontal&quot;, &quot;widescreen&quot;, etc.</li>
  </ul>
  <p>Based on these, here are some takeaways:</p>
  <ul>
    <li><b>Only English prompts work.</b> The models are very small, they were only trained on a single language. Using any other language will result in completely random results.</li>
    <li><b>Always save your best results,</b> because VoidMoA won't. There is no trace of any prompt or resulting image, they only exist on the server until they're sent to you, and after that, they only exist in your browser until you close it. The only way to preserve any image is to save it after you've generated it.</li>
    <li><b>Try rapid prototyping.</b> Generating HD images take much longer, 15 seconds instead of 3 seconds per image on an RTX 3060 Mobile. To check if your prompt makes what you actually want, try a few small images without &quot;high-res&quot; or similar
    modifiers in the text, and when the prompt is right, go big.</li>
    <li><b>Try mixed style prompting.</b> You can't be sure if models were tagged using single, comma separated codewords, or full sentences. For example, one of these prompts might be a hit and one might be a miss:</li>
    <ul>
      <li>&quot;maned wolf with wings flying past Jupiter&quot;</li>
      <li>&quot;maned wolf, wings, flying, Jupiter&quot;</li>
    </ul>
    <li><b>Comma-separate full screen modifiers.</b> Heavy changes asked from the generation, such as making the image &quot;high detail&quot; or &quot;HD&quot;, are better separated from the main prompt with a comma. VoidMoA has the ability not to pass some keywords
    to the artists, but it's only possible, if they aren't part of a sentence. Some examples:</li>
    <ul>
      <li>&quot;Junkrat from Overwatch on a space station, cinematic&quot; - this will make sure you'll get a widescreen image</li>
      <li>&quot;fox puppy in a Tennis match crowd, vertical&quot; - this will make you a cute mobile phone wallpaper</li>
      <li>&quot;screenshot of an FPS game on PC, gameplay, widescreen&quot;</li>
    </ul>
    <li><b>Fair use.</b> For transformative reasons, some artists allow prompts containing works under copyright. However, sometimes a specific codeword might be needed to tell what it is. For example, most cars are known by the models, but unless it's stated that it's a car, the image might be fully random. Some examples:</li>
    <ul>
      <li>&quot;photo of a Rimac Nevera car exploding&quot;</li>
      <li>&quot;Samuel Jackson in How to Train Your Dragon&quot;</li>
    </ul>
  </ul>
  <a class="btn btn-success mb-3" href="image.php">Cool, let's make some images!</a>
</div>
</body>
</html>
