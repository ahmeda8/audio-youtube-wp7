using System;

namespace RTSP
{
    public class SETUP
    {
        public string OriginalResponse { get; set; }
        public string Status { get; set; }
        public string Session { get; set; }
        public string Transport { get; set; }
        public int Server_RTP_Port { get; set; }
        public int Server_RTCP_Port { get; set; }
        public string Source { get; set; }
        public string SSRC { get; set; }
        public int Client_RTP_Port { get; set; }
        public int Client_RTCP_Port { get; set; }

        public SETUP(string Response)
        {
            this.OriginalResponse = Response;
            ParseResponse();
        }

        public void ParseResponse()
        { 
            string[] pattern = {"\r\n"};
            string[] msgSplit = this.OriginalResponse.Split(pattern, StringSplitOptions.None);
            char[] frameSplitter = {':'};
            string[] frameSplit;
            string[] ports;
            Status = msgSplit[0];
            foreach (string frameUnit in msgSplit)
            {
                frameSplit = frameUnit.Split(frameSplitter);
                char[] substringSplitter = {';'};
                switch (frameSplit[0])
                {
                     
                    case "Session" :
                        Session = frameSplit[1].Split(substringSplitter)[0];
                        break;
                    case "Transport" :
                        string[] substringSplit = frameSplit[1].Split(substringSplitter);
                        Transport = frameSplit[1];
                        foreach(string transportUnit in substringSplit)
                        {
                            string []transportSubstring = transportUnit.Split('=');
                            switch (transportSubstring[0])
                            {
                                case "server_port":
                                    ports = transportSubstring[1].Split('-');
                                    Server_RTP_Port = int.Parse(ports[0]);
                                    Server_RTCP_Port = int.Parse(ports[1]);
                                    break;
                                case "client_port":
                                    ports = transportSubstring[1].Split('-');
                                    Client_RTP_Port = int.Parse(ports[0]);
                                    Client_RTCP_Port = int.Parse(ports[1]);
                                    break;
                                case "source":
                                    Source = transportSubstring[1];
                                    break;
                                case "ssrc":
                                    SSRC = transportSubstring[1];
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }
}
