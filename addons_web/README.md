# Addons for the Website
This is the folder of example addons. In each subfolder, an addon is present
with description, install instructions, and source code. Examples are:
* [Censor](./Censor/README.md): prevents image generation with some words.
* [Usage tracker](./Usage%20tracker/README.md): graphs server load over time.

## Development
The website features extension points where a PHP script is called when it
exists. These extension points will have access to the following global
variables:
* `$sqlink`: access to the database through a MySQLi object.
* `$admin`: true if the user has administrator access.
* `$uid`: unique identifier of the user.

**Important:** when you create an addon that requires an update to the Terms of
Service or the Privacy Policy, update those too.

The extension points can be inserted to a set of locations. Use `<location
name.php>` as a file name, where locations names can be the following:
* `check`: before a check command (POST request) is processed and status is
  reported.
* `command_after`: after a command command (POST request) was processed and a
  command was inserted to the queue.
* `command_before`: before a command command (POST request) is processed and a
  command is inserted to the queue.
* `image`: before the image prompting form.
