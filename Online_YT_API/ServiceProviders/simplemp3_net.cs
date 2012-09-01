using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Resources
{
    public class simplemp3_net : WebMethod,IOnlineServiceProvidersApi,IDisposable
    {
        string Base_URL = "http://www.simplemp3.net/download.php?quality=64&submit=Create+MP3+File&url=";
        public string Host_URL { get; set; }
        public string Youtube_Source_URL{get;set;}
        public event ApiCompletedEventHandler Completed;
        public event ApiCompletedEventHandler Failed;

        private void RaiseCompleted(APICompletedEventArgs e)
        {
            if(Completed != null)
                Completed(this,e);
        }

        private void RaiseFailed(APICompletedEventArgs e)
        {
            ErrorLogging.Log(this.GetType().ToString(), e.Response, "ServiceProviderError", HttpUtility.UrlEncode(this.Youtube_Source_URL));
            if (Failed != null)
                Failed(this, e);
        }

        public void GetStreamURL(string URL)
        {
            Host_URL = "http://www.simplemp3.net";
            string req = Base_URL + HttpUtility.UrlEncode(URL);
            this.Youtube_Source_URL = URL;
            base.GET(req);
        }

        public override void GET_Method_CallBack(IAsyncResult res)
        {
            try
            {
                HttpWebRequest wr = (HttpWebRequest)res.AsyncState;
                HttpWebResponse response = (HttpWebResponse)wr.EndGetResponse(res);
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string str = sr.ReadToEnd();
                Regex reg = new Regex("/download.php\\?mp3=(.*)\"+");
                Match m = reg.Match(str);
                int len = m.Length;
                if (len != 0)
                    len--;
                string download_url = Host_URL + str.Substring(m.Index, len);
                if (download_url.Length > Host_URL.Length)
                {
                    RaiseCompleted(new APICompletedEventArgs(download_url));
                }
                else
                {
                    throw new Exception("SimpleMP3.net failed");
                }
            }
            catch (Exception e)
            {
                RaiseFailed(new APICompletedEventArgs(e.Message));
            }
        }

        public override void POST_Method_CallBack(IAsyncResult res)
        {
            throw new NotImplementedException();
        }

        
    }
}
