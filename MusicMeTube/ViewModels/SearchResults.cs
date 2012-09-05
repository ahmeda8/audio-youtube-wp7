using System;
using System.Net;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json.Linq;
using Resources;

namespace MusicMeTube
{
    public class SearchResults : WebMethod
    {
        int start_index = 1;
        int max_result = 50;
        string query;
        private ObservableCollection<Entry> _results = new ObservableCollection<Entry>();
        public ObservableCollection<Entry> Results 
        {
            get { return _results; }
            set { _results = value; }
        }

        public SearchResults(string query_terms)
        {
            query = HttpUtility.UrlEncode(query_terms);
        }

        public void Next()
        {
            string url = "http://gdata.youtube.com/feeds/api/videos?"+
                         "alt=json" +
                         "&q="+query+
                         "&start-index=" + start_index.ToString()+
                         "&max-results="+max_result.ToString()+
                         "&v=2";
            start_index += max_result;
            GET(url);
        }

        public void Previous()
        {
            start_index -= max_result;
            string url = "http://gdata.youtube.com/feeds/api/videos?" +
                         "alt=json"+
                         "&q=" + query +
                         "&start-index=" + start_index.ToString() +
                         "&max-results=" + max_result.ToString() +
                         "&v=2";
            GET(url);
        }

        public override void GET_Method_CallBack(IAsyncResult res)
        {
            JObject apijson = new JObject();
            try
            {
                HttpWebRequest wr = (HttpWebRequest)res.AsyncState;
                HttpWebResponse response = (HttpWebResponse)wr.EndGetResponse(res);
                StreamReader sr = new StreamReader(response.GetResponseStream());
                apijson = JObject.Parse(sr.ReadToEnd());

            }
            catch (Exception ex)
            {
                ErrorLogging.Log(this.GetType().ToString(), ex.Message, "SearchResults", string.Empty);
            }
            PopulateCollection(apijson);
        }

        private void PopulateCollection(JObject json)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    foreach (JObject entry in json["feed"]["entry"])
                    {
                        Entry newent = new Entry();
                        newent.Id = (string)entry["media$group"]["yt$videoid"]["$t"];
                        newent.Title = (string)entry["title"]["$t"];
                        newent.ImageSource = (string)entry["media$group"]["media$thumbnail"][0]["url"];
                        string dura = (string)entry["media$group"]["yt$duration"]["seconds"];
                        newent.Duration = TimeSpan.FromSeconds(double.Parse(dura));
                        newent.Credit = (string)entry["media$group"]["media$credit"][0]["yt$display"];
                        Results.Add(newent);
                    }
                }
                catch (Exception e)
                {
                    ErrorLogging.Log(this.GetType().ToString(), e.Message, "SearchResults", string.Empty);
                }
            });
        }

        public override void POST_Method_CallBack(IAsyncResult res)
        {
            throw new NotImplementedException();
        }
    }
}
