using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace AudioPlaybackAgent
{
    public class YTSourceEventArgs : EventArgs
    {
        public string SOURCE;
        public YTSourceEventArgs(String Source)
        {
            this.SOURCE = Source;
        }
    }

    //public delegate void StreamReadyEventHandler(object sender, StreamReadyEvntArgs e);
    public delegate void SourceReadyEventHandler(object sender, YTSourceEventArgs e);

    public class youtube_org_api
    {
        public string AckCode { get; set; }
        string api_req_url = "http://linesoftware.webege.com/music/test1.php?yturl=";
        HttpWebRequest wr;
        public event SourceReadyEventHandler Completed;
        
        public youtube_org_api()
        {

        }

        protected virtual void RaiseSourceAvailable(YTSourceEventArgs e)
        {
            if (Completed != null)
                Completed(this, e);
        }
        public void Download(String URL)
        {

            URL = HttpUtility.UrlEncode(URL);
            string req_src = api_req_url + URL;
            wr = (HttpWebRequest)HttpWebRequest.Create(new Uri(req_src, UriKind.Absolute));
            wr.BeginGetResponse(new AsyncCallback(req_callback), wr);
        }

        private void req_callback(IAsyncResult result)
        {
            HttpWebRequest nwr = (HttpWebRequest)result.AsyncState;
            HttpWebResponse rs = (HttpWebResponse)nwr.EndGetResponse(result);//wr.EndGetResponse(result) as HttpWebResponse;
            StreamReader api_resp = new StreamReader(rs.GetResponseStream());
            AckCode = api_resp.ReadToEnd();
            RaiseSourceAvailable(new YTSourceEventArgs(AckCode));
        }
    }
}