using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Resources
{
    public class video2mp3_at : WebMethod, IOnlineServiceProvidersApi,IDisposable
    {


        public string Host_URL { get; set; }
        private string yt_id;
        private int trycount = 0;
        public string Youtube_Source_URL{get;set;}
        public event ApiCompletedEventHandler Completed;
        public event ApiCompletedEventHandler Failed;
        private Stage CurrentStage;
        private enum Stage
        {
            CHECK,
            CONVERT,
            DOWNLOAD,
            WAIT
        }

        private void RaiseCompleted(APICompletedEventArgs e)
        {
            if (Completed != null)
                Completed(this, e);
        }

        private void RaiseFailed(APICompletedEventArgs e)
        {
            ErrorLogging.Log(this.GetType().ToString(), e.Response, "ServiceProviderError", HttpUtility.UrlEncode(this.Youtube_Source_URL));
            if (Failed != null)
                Failed(this, e);
        }
        
        public void GetStreamURL(string URL)
        {
            Host_URL = "http://www.video2mp3.at/";
            string[] split = URL.Split('=');
            Youtube_Source_URL = URL;
            yt_id = split[1];
            Random rand = new Random();
            string url = Host_URL + "check.php?check=checkit&id=" + yt_id + "&key=" + Math.Floor((double)(rand.Next(0, 100) * 35000));
            trycount = 0;
            base.GET(url);
            CurrentStage = Stage.CHECK;
        }

       
        public override void GET_Method_CallBack(IAsyncResult res)
        {
            try
            {
                HttpWebRequest wr = (HttpWebRequest)res.AsyncState;
                HttpWebResponse response = (HttpWebResponse)wr.EndGetResponse(res);
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string str = sr.ReadToEnd();

                Random rand = new Random();
                Regex reg = new Regex("http://(.*)\\.video2mp3.at/settings.php\\?set=download&id=(.*)\"+");
                Match m = reg.Match(str);
                int len = m.Length;
                if (len != 0)
                    len--;
                string download_url = str.Substring(m.Index, len);
                if (download_url.Length > 0)
                {
                    String StreamURL = download_url;
                    RaiseCompleted(new APICompletedEventArgs(download_url));
                    return;
                }
                if (trycount > 10)
                {
                    throw new TimeoutException("Retries :" + trycount);
                }
                string[] responses = str.Split('|');
                switch (CurrentStage)
                {
                    case Stage.CHECK:
                        if (responses[0] == "OK")
                        {
                            GET(Host_URL + "get/" + yt_id + "/");
                            CurrentStage = Stage.DOWNLOAD;
                        }
                        else
                        {
                            string url = Host_URL + "check.php?hash=" + Math.Floor((double)(rand.Next(0, 100) * 35000)) + "&yt=" + HttpUtility.UrlEncode(Youtube_Source_URL);
                            GET(url);
                            CurrentStage = Stage.CONVERT;
                        }
                        break;
                    case Stage.CONVERT:
                        if (responses[0] == "YES")
                            GET(Host_URL + "get/" + yt_id + "/");
                        else
                        {
                            string url = Host_URL + "check.php?check=checkit&id=" + yt_id + "&key=" + Math.Floor((double)(rand.Next(0, 100) * 35000));
                            GET(url);
                            trycount++;
                            System.Threading.Thread.Sleep(5000);
                            CurrentStage = Stage.CONVERT;
                        }
                        break;
                    case Stage.DOWNLOAD:
                        //if I reach here that means regex failed
                        RaiseFailed(new APICompletedEventArgs("regexfail=1 output:" + str));
                        break;
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
