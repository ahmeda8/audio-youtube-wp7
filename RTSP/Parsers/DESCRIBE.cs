using System;
using System.Net;

namespace RTSP
{
    public class DESCRIBE
    {
        public string OriginalResponse { get; set; }
        public string Scheme { get; set; }
        public string Status { get; set; }
        public string ContentType { get; set; }
        public string CacheControl { get; set; }
        public string Date { get; set; }
        public string Expires { get; set; }
        public string LastModified { get; set; }
        public string ContentBase { get; set; }
        public string Server { get; set; }
        public int ContentLength { get; set; }
        public int VideoH { get; set; }
        public int VideoW { get; set; }
        public string VideoStream { get; set; }
        public string VideoCC { get; set; }
        public int VideoProfileId { get; set; }
        public string AudioStream { get; set; }
        public string AudioCodecPrivate { get; set; }
        public int AudioProfileId { get; set; }
        public string Range { get; set; }
        public string[] Streams { get; set; }

        public DESCRIBE(string Message)
        {
            this.OriginalResponse = Message;
            Streams = new string[2];
            ParseMessage(this.OriginalResponse);
        }
        private void ParseMessage(string msg)
        {
            string[] pattern = {"\r\n"};
            char[] frameSplitter = { ':' };
            string[] msgSplit = msg.Split(pattern,StringSplitOptions.None);
            string[] subStrings;
            Status = msgSplit[0];
            if (!Status.Contains("RTSP/1.0 200 OK")) throw new WebException(Status);
            foreach (string frameUnit in msgSplit)
            {
                
               subStrings = frameUnit.Split(frameSplitter);

                switch (subStrings[0])
                {
                    case "Content-Type":
                        ContentType = subStrings[1];
                        break;
                    case "Cache-Control":
                        CacheControl = subStrings[1];
                        break;
                    case "Date":
                        Date = subStrings[1];
                        break;
                    case "Expires":
                        Expires = subStrings[1];
                        break;
                    case "Last-Modified":
                        LastModified = subStrings[1];
                        break;
                    case "Content-Base":
                        ContentBase = subStrings[1]+":"+subStrings[2];
                        break;
                    case "Server":
                        Server = subStrings[1];
                        break;
                    case "Content-Length":
                        ContentLength = int.Parse(subStrings[1]);
                        break;
                    case "a=range":
                        Range = subStrings[1];
                        break;
                    
                }

                
            }
            int videoindex = msg.IndexOf("m=video");
            int audioindex = msg.IndexOf("m=audio");
            string videoInfo = msg.Substring(videoindex, audioindex - videoindex);
            string audioInfo = msg.Substring(audioindex, msg.Length - audioindex);

            msgSplit = videoInfo.Split(pattern,StringSplitOptions.None);
            foreach (string frameUnit in msgSplit)
            {
                subStrings = frameUnit.Split(frameSplitter);
                switch (subStrings[0])
                {
                    case "a=control":
                        VideoStream = subStrings[1];
                        Streams[1] = subStrings[1];
                        break;
                }
            }

            msgSplit = audioInfo.Split(pattern, StringSplitOptions.None);
            foreach (string frameUnit in msgSplit)
            {
                subStrings = frameUnit.Split(frameSplitter);
                switch (subStrings[0])
                {
                    case "a=control":
                        AudioStream = subStrings[1];
                        Streams[0] = subStrings[1];
                        break;
                    case "a=fmtp":
                        //AudioCodecPrivate = subStrings[1].Substring(41, 12);
                        break;
                }
            }
            //Console.WriteLine(ContentBase);
        }

    }
}
