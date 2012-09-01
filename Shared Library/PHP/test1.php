<?php
$yt_url = urldecode($_GET['yturl']);
$api_url = "http://www.youtube-mp3.org/api/pushItem/?item=".urlencode($yt_url)."&xy=yx";
$a="";
$a = file_get_contents($api_url);
if(strlen($a)==0)
{
exit("error in a");
}
$api_url_2 = "http://www.youtube-mp3.org/api/itemInfo/?video_id=".$a;
$b = file_get_contents($api_url_2);
if(strlen($b)==0)
exit("error in b");
$len = strlen($b);
$len = $len - 8;
$json = substr($b,7,$len);
$b = json_decode($json,true);
$tries=0;
while($b['status'] != "serving" && $tries < 5 && $a != "")
{
        sleep(2);
        $b = file_get_contents($api_url_2);
        $json = substr($b,7,(strlen($b)-8));
        $b = json_decode($json,true);
        $tries++;
}

$api_url_3 = "http://www.youtube-mp3.org/get?video_id=".$a."&h=".$b['h'];
echo $api_url_3;
//echo "<br/> Genric Page";
//$generic = file_get_contents("http://www.youtube-mp3.org/?c#v=".$a);
//echo $generic;
exit();
//header("Location: ".$api_url_3);
//header('Content-Type: audio/mpeg3');
//readfile($file)
//file_put_contents("temp.mp3",file_get_contents($api_url_3));
?>
