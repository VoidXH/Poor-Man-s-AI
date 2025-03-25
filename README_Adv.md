# Poor Man's AI: advanced configuration
## Conversation starters
These are the example buttons visible when you open the chat on the Website.
They are contained in the Website's `__chat.php` file. Open it with any text
editor, and make a copy of or change any example lines. If you don't know PHP,
just make sure your new line looks just like the others, in the ["title",
"message"] format. One thing to note: when you type the `'` character, it can
break the website's code when it's inserted, so type `\'` instead.
  
## Improving chat knowledge base
Extra knowledge can easily be added to models by copying documents containing
said knowledge to the Context Doc Tree: each subfolder is a new keyword the
prompt has to contain, and the file name is the final trigger. Let's say the
user asks:
```
How do I decode object-based audio files with Cavern?
```
Let's say that in the Context Doc Tree folder set in the configuration, there is
a `Cavern` subfolder. The parser finds it, and checks its contents: there's
`DCP.html`, `Listener.md`, and `object.txt` inside. We have a match:
`object.txt`, which will become part of the prompt, and any running model can
learn from it.

What if the needed context is in a different file? That requires a large context
window. If the context window is large enough, all files from all subfolders
until the final keyword is reached can be loaded by editing the configuration
accordingly. You can also set multiple keywords to a file, for example, calling
one `license, licence, terms.md`.

## Adding a chat window to your website
This task requires developer knowledge and a PHP-based website. First, include
`chat_insert.php` from your Website installation. There are two functions
available for different use cases. The complete chat handling, putting a chat
button to the bottom right of your website is:
```php
aiChatPopup($pmaiPath, $name, $modelName, $fullInclude);
```
To put the chat in your custom container, call this function from its `div`:
```php
aiChat($pmaiPath, $name, $modelName, $fullInclude);
```
Parameters are the following:
* `$pmaiPath`: URL path from the calling folder to your Website installation. To
  calculate from the server root, just add a `/` prefix.
* `$name`: How to call your chatbot.
* `$modelName`: Which model to use on the Processor for chatting with this bot.
* `$fullInclude`: If your website is in Bootstrap and is already including
  Bootstrap's CSS + JS and JQuery, set this to `false`. If set to `true`, these
  will also be included next to the chat dependencies.

## Using multiple Processors from multiple computers
The `ChatWeight` and `ImageGenWeight` parameters are shared between computers.
If you launch the Processor on one computer with a weight of 2, and with 1 on
another, only the higher weight will process anything. When setting a weight to
-1, it will not process anything. You can set up image-only and chat-only nodes
this way. When multiple computers log in with the same weight, they share the
load for the corresponding engine.

## Miscellaneous
* To add a new background to the chat or image generator components on your
  website, just go to its img folder, and create new diffbgXX.jpg or gptbgXX.jpg
  files with the images you want to use. XX is a number one larger than the
  previous largest in the folder.
* Developers can easily add addons to the Website's predefined extension points
  to easily publish them in a compatible format. More about addons in
  [their folder](./addons_web/README.md).