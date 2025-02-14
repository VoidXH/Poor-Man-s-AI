<?php
require("../sql.php");
$install = "CREATE TABLE IF NOT EXISTS `ai_usage` (
  `time` timestamp NOT NULL,
  `usage` int(11) DEFAULT '0',
  PRIMARY KEY (`time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8";
if ($sqlink->query($install)) {
  echo "Usage tracker installed successfully.";
} else {
  echo "Installation failed for reason: " . $sqlink->error;
}
?>
