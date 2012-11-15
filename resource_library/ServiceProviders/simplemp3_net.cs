using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace ResourceLibrary
{
    public class simplemp3_net : WebMethod,IOnlineServiceProvidersApi,IDisposable
    {
        string Base_URL = "http://www.simplemp3.net/index.php";
        public string Host_URL { get; set; }
        public string Youtube_Source_URL{get;set;}
        public event ApiCompletedEventHandler Completed;
        public event ApiCompletedEventHandler Failed;

        private string Unique_id;
        private string mp3File;
        private int conversion_log_length = 0;
        private int STAGE = 0;
        

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
            STAGE = 0;
            Host_URL = "http://www.simplemp3.net";
            string req = Base_URL + HttpUtility.UrlEncode(URL);
            this.Youtube_Source_URL = URL;
            string msgparams = "quality=64&submit=Create+MP3+File&youtubeURL="+this.Youtube_Source_URL;
            base.POST(Base_URL,msgparams);
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
            //need to find the uniqueid
            try 
            {
                HttpWebRequest wr = (HttpWebRequest)res.AsyncState;
                HttpWebResponse response = (HttpWebResponse)wr.EndGetResponse(res);
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string str = sr.ReadToEnd();
                    
                if (STAGE == 0)
                {
                    Regex reg = new Regex("uniqueId=(.*)&logLength=");
                    Match m = reg.Match(str);
                    int len = m.Length;
                    Unique_id = m.Value;
                    string pattern_filename = "updateConversionProgress" + Regex.Escape("(") + "\"(.*)\"" + Regex.Escape(")") + ";";
                    reg = new Regex(pattern_filename);
                    Match m_filename = reg.Match(str);
                    mp3File = m_filename.Value.Substring(26, m_filename.Length - 26 - 3);
                    string msg = Unique_id + conversion_log_length + "&mp3File=" + mp3File;
                    STAGE = 1; //set to next stage
                    base.POST(Host_URL + "/ffmpeg_progress.php", msg);
                    
                }
                else if (STAGE == 1)
                {
                    string[] retVals = str.Split('|');
                    if (int.Parse(retVals[3]) == 2)
                    {
                        if (int.Parse(retVals[1]) < 100)
                        {
                            conversion_log_length = int.Parse(retVals[0]);
                            System.Threading.Thread.Sleep(5000);
                            string msg = Unique_id + conversion_log_length + "&mp3File=" + mp3File;
                            base.POST(Host_URL + "/ffmpeg_progress.php", msg);
                        }
                        else if (int.Parse(retVals[2]) == 1)
                        {
                            RaiseCompleted(new APICompletedEventArgs(Base_URL+"?mp3=" + HttpUtility.UrlEncode(mp3File)));
                        }
                        else
                        {
                            RaiseFailed(new APICompletedEventArgs("Conversion failed at simple mp3 net"));
                        }
                    }
                }
 
            }
            catch (Exception e)
            {
                RaiseFailed(new APICompletedEventArgs(e.Message));
            }
        }

        
    }
}
