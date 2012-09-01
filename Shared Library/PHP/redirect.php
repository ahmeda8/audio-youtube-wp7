<?php 
$api_url = urldecode($_GET['apiurl']);
header("Location: $api_url");
exit();
?>