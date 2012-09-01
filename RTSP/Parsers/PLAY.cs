using System;

namespace RTSP
{
    public class PLAY
    {
        public string OriginalResponse { get; set; }
        public string Status { get; set; }
        public string Session { get; set; }
        public string Transport { get; set; }
        public Uri Url { get; set; }
        


        public PLAY(string Response)
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
                    case "RTP-Info" :
                        Url = new Uri("rtsp:"+frameSplit[2].Split(substringSplitter)[0],UriKind.Absolute);
                        break;
                }
            }
        }
    }
}
