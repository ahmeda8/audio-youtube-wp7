using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.IO;
using Resources;

namespace MusicMeTube
{
    public class ViewModelTracklist : WebMethod
    {
        public ObservableCollection<Entry> tracklistentry { set; get; }
        Entry plentry;
        private JObject apijson;
        public bool completed = false;
        
        public ViewModelTracklist(Entry plentry)
        {
           this.plentry = plentry;
           tracklistentry = new ObservableCollection<Entry>();
           completed = false;
           GetApiResponse();

        }

        public void GetApiResponse()
        {
            string filename = "cache/" + plentry.Id + ".json";
            if (ISOHelper.FileExists(filename))
            {   
                apijson = JObject.Parse(ISOHelper.ReadFromFile(filename));
                PopulateCollection(apijson);
            }
            else
            {
                string url = plentry.Source + "&alt=json&access_token=" + IsolatedStorageSettings.ApplicationSettings["access_token"];
                GET(url);
            }
            
        }

        private void PopulateCollection(JObject response)
        {
            
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    string id = "";
                    foreach (JObject entry in response["feed"]["entry"])
                    {
                        Entry newent = new Entry();
                        newent.PlaylistID = plentry.Id;
                        newent.Id = (string)entry["media$group"]["yt$videoid"]["$t"];
                        newent.Title = (string)entry["title"]["$t"];
                        newent.ImageSource = (string)entry["media$group"]["media$thumbnail"][0]["url"];
                        string dura = (string)entry["media$group"]["yt$duration"]["seconds"];
                        newent.Duration = TimeSpan.FromSeconds(double.Parse(dura));
                        foreach (JObject jo in entry["link"])
                        {
                            string src;
                            if ((string)jo["type"] == "text/html" && (string)jo["rel"] == "alternate")
                            {
                                src = (string)jo["href"];
                                string[] src_split = src.Split('&');
                                newent.Source = src_split[0];
                                break;
                            }
                        }
                        if (newent.Source == null)
                            throw new WebException("Cant get Video Address from API");
                        id = newent.Id;
                        string filename = plentry.Id + "\\" + id + ".mp3";
                        string offline = "Not Synced";
                        SolidColorBrush col = new SolidColorBrush(Colors.Red);

                        if (ISOHelper.FileExists(filename))
                        {
                            offline = "Available offline";
                            col.Color = Colors.Green;
                        }
                        newent.AvailablityColor = col;
                        newent.Offline = offline;
                        tracklistentry.Add(newent);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogging.Log(this.GetType().ToString(), ex.Message, "ViewModelTracklist", "probablyJSONResponse");
                }
            });
            completed = true;
        }


        public override void GET_Method_CallBack(IAsyncResult res)
        {
            try
            {
                HttpWebRequest wr = (HttpWebRequest)res.AsyncState;
                HttpWebResponse response = (HttpWebResponse)wr.EndGetResponse(res);
                StreamReader sr = new StreamReader(response.GetResponseStream());
                apijson = JObject.Parse(sr.ReadToEnd());
                if (apijson != null)
                {
                    string filename = "cache/" + plentry.Id + ".json";
                    ISOHelper.WriteToFile(filename, apijson.ToString());
                }

            }
            catch (Exception ex)
            {
                ErrorLogging.Log(this.GetType().ToString(),ex.Message,"ViewModelTracklist",string.Empty);
            }
            PopulateCollection(apijson);
        }

        public override void POST_Method_CallBack(IAsyncResult res)
        {
            throw new NotImplementedException();
        }
    }
}
