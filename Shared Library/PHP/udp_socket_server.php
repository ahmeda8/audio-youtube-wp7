<?php
echo "Creating Socket\n";
$sock = socket_create(AF_INET,SOCK_DGRAM,getprotobyname("udp"));
echo "Socket created \n";
$bind = socket_bind($sock,"172.16.113.56",22222);

if($bind)
	echo "Bind Succesfull\n";
else
	echo "Bind Unsucessfull\n";

$buffer = "";
$remote_ip = "";
$remote_port = "";
echo "Receive from...\n";
while($buffer != "END")
{
socket_recvfrom($sock,$buffer,1024,0,$remote_ip,$remote_port);

echo "Received $buffer from remote address $remote_ip and remote port $remote_port" . PHP_EOL;

echo "Sending...";

$send = "$buffer mee too";
socket_sendto($sock,$send,strlen($send),0,$remote_ip,$remote_port);

echo "Sent $send\n";
}
?>
