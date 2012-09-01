using System;
using System.IO;
using System.Net;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace Resources
{
    public class youtube_org : WebMethod,IOnlineServiceProvidersApi,IDisposable
    {
        public string Youtube_Source_URL { get; set; }
        public event ApiCompletedEventHandler Completed;
        public event ApiCompletedEventHandler Failed;
        public string Host_URL { get; set; }

        private String Push_Url = "api/pushItem/?item=";
        private String Check_Url = "api/itemInfo/?video_id=";
        private String Download_Url = "get?video_id=";
        private int trycount;
        private Stage CurrentStage;
        private string video_id;
        
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

        
        public void GetStreamURL(string Youtube_URL)
        {
            Host_URL = "http://www.youtube-mp3.org/";
            trycount = 0;
            Youtube_Source_URL = Youtube_URL;
            Random rand = new Random();
            int random = rand.Next(0, 100) * 350000;
            string Req_Url = Host_URL + Push_Url + HttpUtility.UrlEncode(Youtube_URL) + "&xy=yx&bf=false&r=" + Math.Floor(random);
            CurrentStage = Stage.CONVERT;
            GET(Req_Url);
        }

        public override void GET_Method_CallBack(IAsyncResult res)
        {
            try
            {
                HttpWebRequest wr = (HttpWebRequest)res.AsyncState;
                HttpWebResponse response = (HttpWebResponse)wr.EndGetResponse(res);
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string str = sr.ReadToEnd();
                string url;
                Random rand = new Random();
                int random = rand.Next(0, 100) * 350000;

                switch (CurrentStage)
                {
                    case Stage.CONVERT:
                        video_id = str;
                        url = Host_URL + Check_Url + video_id + "&ac=www&r=" + random;
                        CurrentStage = Stage.CHECK;
                        base.GET(url);
                        break;
                    case Stage.CHECK:
                        str = str.Substring(7, (str.Length - 8));
                        JObject check_json = JObject.Parse(str);
                        if ((string)check_json["status"] == "serving")
                        {
                            CurrentStage = Stage.DOWNLOAD;
                            url = Host_URL+ Download_Url + video_id + "&h=" + (string)check_json["h"] + "&r=" + random;
                            // StreamURL = url;
                            RaiseCompleted(new APICompletedEventArgs(url));
                        }
                        else if (trycount == 11)
                        {
                            //ErrorLogging.Log("Retries exceeded :" + trycount +" "+ e.Response);
                            throw new TimeoutException("Retries :" + trycount );
                        }
                        else
                        {
                            trycount++;
                            System.Threading.Thread.Sleep(5000);
                            url = Host_URL+ Check_Url + video_id + "&ac=www&r=" + random;
                            GET(url);
                        }
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
