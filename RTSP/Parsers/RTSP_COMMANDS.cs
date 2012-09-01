using System;
using System.Net.Sockets;


namespace RTSP
{

    public class RTSP_COMMANDS
    {
        public enum STREAM_TYPE
        { 
            AUDIO = 1,
            VIDEO = 2
        }

        public static string Describe(String Source)
        {
            string command = "DESCRIBE " + Source + RTSP_CONSTANTS.RTSP_VERSION
                           + Environment.NewLine + "CSeq: " + RTSP_CONSTANTS.Cseq++ + Environment.NewLine;
            return command;
        }

        public static string Setup(DESCRIBE describe_frame,int client_port,int stream_id)
        {
            
            String command = "SETUP " + describe_frame.ContentBase + describe_frame.Streams[stream_id] + RTSP_CONSTANTS.RTSP_VERSION + Environment.NewLine;
            command += "Transport: RTP/AVP/UDP;unicast;client_port=" + client_port + "-" + (client_port+1) + ";mode=play" + Environment.NewLine;
            command +="CSeq: " + RTSP_CONSTANTS.Cseq++ + Environment.NewLine;
            return command;
        }

        public static string Play(SETUP setup_frame,DESCRIBE describe_frame,int stream_id)
        {
            
            String command = "Play " + describe_frame.ContentBase + describe_frame.Streams[stream_id] + RTSP_CONSTANTS.RTSP_VERSION + Environment.NewLine;
            command +="Range:" + describe_frame.Range + Environment.NewLine;
            command +="Session:" + setup_frame.Session + Environment.NewLine;
            command +="CSeq: " + RTSP_CONSTANTS.Cseq++ + Environment.NewLine;
            return command;
        }

        public static string Teardown(PLAY play_frame,SETUP setup_frame)
        {
            var command = "TEARDOWN " + play_frame.Url.AbsoluteUri + RTSP_CONSTANTS.RTSP_VERSION + Environment.NewLine
                + "Session:" + setup_frame.Session + Environment.NewLine
                + "CSeq: " + RTSP_CONSTANTS.Cseq++ + Environment.NewLine;
            return command;
        }
    }
}
