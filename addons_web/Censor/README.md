# Usage tracker addon
Prevents image generation with prompts that contain selected words or phrases.
Along with negative prompting, this can be a very efficient filter to prevent
adult content being generated. This is required for making your installation
openly available, because most models are uncensored. It's recommended to add as
much adult phrases to the filters as you can think of.

## Installation
1. Open command_before.php in any text editor and change the keywords and
   selectors according to the comments in the file. Each list's entries must be
   in quotes and end with a comma after the closing quote. The list of keywords
   must be in alphabetical order.
1. Copy all files to the addon folder on your server running the Website part of
   this software.
