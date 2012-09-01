using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Resources;
using System.IO.IsolatedStorage;

namespace RTSP
{
    public class RTSP_FileDownloader
    {
        private Socket UDP_SOCKET;
        private Socket TCP_SOCKET;

        private SocketAsyncEventArgs Udp_Socket_EvntArgs;
        public SocketAsyncEventArgs Tcp_Socket_EvntArgs;

        private const int PORT_SERVER_PORT = 22222;
        private const int MAX_BUFFER_SIZE = 8192;
        private const string PORT_SERVER_ADDRESS = "192.168.0.3";
        private int UDP_PORT;

        enum COMMAND
        {
            DESCRIBE,
            SETUP,
            PLAY,
            PAUSE,
            TEARDOWN
        }
        private COMMAND CurrentCommand;

        private DESCRIBE DescribeFrame;
        private SETUP SetupFrame;
        private PLAY PlayFrame;

        private Entry YoutubeSource;
        private Uri SourceURI;

        private RTP RTP_PARSER;
        private IsolatedStorageFileStream isf;
        private IsolatedStorageFile iso;

        public RTSP_FileDownloader()
        {
#if DEBUG
            YoutubeSource = new Entry { Source = "rtsp://v4.cache3.c.youtube.com/CjYLENy73wIaLQkYYgpvIO6OthMYDSANFEIJbXYtZ29vZ2xlSARSBWluZGV4YOGb4L2jkOqFUAw=/0/0/0/video.3gp" };
#endif
            CurrentCommand = COMMAND.DESCRIBE;
            TCP_SOCKET = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            UDP_SOCKET = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            SourceURI = new Uri(YoutubeSource.Source, UriKind.Absolute);
        
            Udp_Socket_EvntArgs = new SocketAsyncEventArgs();
            Udp_Socket_EvntArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(PORT_SERVER_ADDRESS), PORT_SERVER_PORT);
            var send_buffer = Encoding.UTF8.GetBytes("Connect;LoopBack;");
            Udp_Socket_EvntArgs.SetBuffer(send_buffer, 0, send_buffer.Length);
            Udp_Socket_EvntArgs.Completed += new EventHandler<SocketAsyncEventArgs>(RTP_Socket_EvntArgs_Completed);

            Tcp_Socket_EvntArgs = new SocketAsyncEventArgs();
            Tcp_Socket_EvntArgs.RemoteEndPoint = new DnsEndPoint(SourceURI.Host, RTSP_CONSTANTS.RTSP_SERVER_PORT);
            Tcp_Socket_EvntArgs.SetBuffer(0, MAX_BUFFER_SIZE);
            //Tcp_Socket_EvntArgs.RemoteEndPoint = new DnsEndPoint("rtsp://www.youtube.com", 554);
            Tcp_Socket_EvntArgs.Completed += new EventHandler<SocketAsyncEventArgs>(RTSP_Socket_EvntArgs_Completed);

            RTP_PARSER = new RTP();
        }

        //Completed event args go here
        void RTSP_Socket_EvntArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    SendMessage(RTSP_COMMANDS.Describe(YoutubeSource.Source));
                    break;
                case SocketAsyncOperation.Send:
                    ReceiveMessage();
                    break;
                case SocketAsyncOperation.Receive:
                    var message = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
                    var Messsage_Status = message.Substring(0, 15);
                    if (!Messsage_Status.Contains(RTSP_CONSTANTS.STATUS_OK))
                    {
                        throw new WebException(message);
                    }
                    switch (CurrentCommand)
                    {
                        case COMMAND.DESCRIBE:
                            DescribeFrame = new DESCRIBE(message);
                            CurrentCommand = COMMAND.SETUP;
                            SendMessage(RTSP_COMMANDS.Setup(DescribeFrame,UDP_PORT,0));
                            break;
                        case COMMAND.SETUP:
                            SetupFrame = new SETUP(message);
                            CurrentCommand = COMMAND.PLAY;
                            SendMessage(RTSP_COMMANDS.Play(SetupFrame, DescribeFrame, 0));
                            break;
                        case COMMAND.PLAY:
                            PlayFrame = new PLAY(message);

                            iso = IsolatedStorageFile.GetUserStoreForApplication();
                            isf = new IsolatedStorageFileStream("track.aac", System.IO.FileMode.Create, iso);
                            isf.Close();
                            Udp_Socket_EvntArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, SetupFrame.Server_RTP_Port);
                            Udp_Socket_EvntArgs.SetBuffer(new byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);
                            UDP_SOCKET.ReceiveFromAsync(Udp_Socket_EvntArgs);
                            break;
                        case COMMAND.PAUSE:
                            break;
                        case COMMAND.TEARDOWN:
                            SendMessage(RTSP_COMMANDS.Teardown(PlayFrame, SetupFrame));
                            break;
                    }
                    break;
            }
        }

        void RTP_Socket_EvntArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.SendTo:
                    Udp_Socket_EvntArgs.SetBuffer(new byte[MAX_BUFFER_SIZE],0, MAX_BUFFER_SIZE);
                    Udp_Socket_EvntArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any,PORT_SERVER_PORT);
                    UDP_SOCKET.ReceiveFromAsync(Udp_Socket_EvntArgs);
                    break;
                case SocketAsyncOperation.ReceiveFrom:
                    switch(CurrentCommand)
                    {
                        case COMMAND.DESCRIBE:
                            var message = Encoding.UTF8.GetString(e.Buffer, 0, e.Buffer.Length);
                            UDP_PORT = int.Parse(message);
                            InitiateRTSPRequest();
                            break;
                        case COMMAND.PLAY:
                            RTP_PARSER.Save(e.Buffer,e.BytesTransferred);
                            isf = new IsolatedStorageFileStream("track.amr", System.IO.FileMode.Append, iso);
                            //RTP.RTP_HEADER header = RTP_PARSER.HEADER;
                            isf.Write(RTP_PARSER.PAYLOAD, 0, RTP_PARSER.PAYLOAD.Length);
                            isf.Close();
                            UDP_SOCKET.ReceiveFromAsync(Udp_Socket_EvntArgs);
                            
                            break;
                    }
                    break;
            }
        }

        //end of completed event args

        public void GetRTPSocketPort()
        {
            UDP_SOCKET.SendToAsync(Udp_Socket_EvntArgs);
        }

        private void InitiateRTSPRequest()
        {
            CurrentCommand = COMMAND.DESCRIBE;
            TCP_SOCKET.ConnectAsync(Tcp_Socket_EvntArgs);
        }

        private void SendMessage(string msg)
        {
            var buffer = Encoding.UTF8.GetBytes(msg + Environment.NewLine);
            Tcp_Socket_EvntArgs.SetBuffer(buffer, 0, buffer.Length);
            TCP_SOCKET.SendAsync(Tcp_Socket_EvntArgs);
        }

        private void ReceiveMessage()
        {
            Tcp_Socket_EvntArgs.SetBuffer(new byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);
            TCP_SOCKET.ReceiveAsync(Tcp_Socket_EvntArgs);
        }
    }
}
