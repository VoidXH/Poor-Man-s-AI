# Usage tracker addon
Allows admins to graph server load (maximum number of commands in queue) over
time. This helps to check if your Processor node is able to keep up with demand
or needs an update. Elevated usage can also be an indicator of DDoS attacks.

## Installation
1. Optionally open command_after.php in any text editor and change the value of
   $range to something else, if you want to change the resolution of usage
   tracking from 15 minutes.
1. Copy all files to the addon folder on your server running the Website part of
   this software.
1. Run <your server>/addon/install.php
1. Delete <your server>/addon/install.php (optional)
