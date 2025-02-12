<?php
function execute($query, ...$params) {
  global $sqlink;
  $stmt = $sqlink->prepare($query);
  $types = "";
  
  foreach ($params as $param) {
    if (is_int($param)) {
      $types .= "i";
    } else {
      $types .= "s";
    }
  }

  $stmt->bind_param($types, ...$params);
  $stmt->execute();
  return $stmt;
}
?>
