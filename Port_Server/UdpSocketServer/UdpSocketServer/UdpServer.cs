using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;


namespace UdpSocketServer {
    public class UdpServer {
        public static ManualResetEvent manualReset = new ManualResetEvent(false);
        static ConcurrentBag<EndPoint> remoteEndpoints = new ConcurrentBag<EndPoint>();
        const int PORT = 22222;
        const int MAX_PENDING_REQUEST = 10;
        static Socket listener;

        public static void Start() {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);

            listener.Bind(new IPEndPoint(IPAddress.Any, PORT));

            Console.WriteLine("Waiting for a connection..." + GetLocalIpAddress());

            while (true) {

                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                StateObject state = new StateObject();
                state.WorkSocket = listener;
                listener.ReceiveFrom(state.Buffer, ref remoteEndPoint);
                var rawMessage = Encoding.UTF8.GetString(state.Buffer);
                var messages = rawMessage.Split(';');

                if (messages.Length > 1) {
                    var command = messages[0];
                    var deviceName = messages[1];
                    Console.WriteLine("Command is received from Device Name +"+deviceName+"+");
                    string[] portno = remoteEndPoint.ToString().Split(':');
                    Send(portno[1],remoteEndPoint);
                    
                }
            }
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e) {
            Console.Clear();
            Console.WriteLine("One player exits from the game");

        }
        private static IPEndPoint GetLocalIpAddress() {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var localIpAddress = new IPEndPoint(ipHostInfo.AddressList
                                        .Where(item => item.AddressFamily == AddressFamily.InterNetwork)
                                        .First(), PORT);
            return localIpAddress;
        }



        private static void Send(String port,EndPoint returnaddress) {
                Send(returnaddress, port);
        }

        private static void Send(EndPoint endPoint, String data) {
            //Thread.Sleep(10000);
            Console.WriteLine("Sending +"+data+"+");
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            listener.SendBufferSize = byteData.Length;
            listener.SendTo(byteData, endPoint);
        }
    }
}



