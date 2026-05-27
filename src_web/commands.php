<?php
/*
  User command handling — routes to cmd/usr/ endpoints
  ----------------------------------------------------
  GET:
    ?check=ID   → cmd/usr/check.php
  POST:
    command     → cmd/usr/command.php
    stop        → cmd/usr/stop.php
*/

$method = $_SERVER['REQUEST_METHOD'];
if ($method === 'GET' && isset($_GET['check'])) {
    require('cmd/usr/check.php');
}
if ($method === 'POST') {
    if (isset($_POST['command'])) {
        require('cmd/usr/command.php');
    }
    if (isset($_POST['stop'])) {
        require('cmd/usr/stop.php');
    }
}
?>
