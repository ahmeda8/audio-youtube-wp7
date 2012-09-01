﻿using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MusicMeTube.Library;
using System.Globalization;
using Resources;
using System.IO;

namespace MusicMeTube
{
    public class ViewModelPlaylist : WebMethod
    {
        public static ObservableCollection<Entry> playlistentry { set; get; }
        private JObject apijson;
        
        public ViewModelPlaylist()
        {
            playlistentry = new ObservableCollection<Entry>();
            GetApiResponse();
        }

        private void GetApiResponse()
        {
            string filename;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("cache_filename", out filename);
            if (ISOHelper.FileExists(filename))
            {   
                apijson = JObject.Parse(ISOHelper.ReadFromFile(filename));
                PopulateCollection(apijson);
            }
            else
            {
                string url = "https://gdata.youtube.com/feeds/api/users/default/playlists?v=2&alt=json&access_token=" + IsolatedStorageSettings.ApplicationSettings["access_token"];
                GET(url);
            }
        }

        private void PopulateCollection(JObject response)
        {
            
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    foreach (JObject entry in response["feed"]["entry"])
                    {
                        Entry newent = new Entry();
                        newent.Id = (string)entry["yt$playlistId"]["$t"];
                        newent.Title = (string)entry["title"]["$t"];
                        newent.Source = (string)entry["content"]["src"];
                        newent.Title = StringExtension.TitleCase(newent.Title);
                        newent.ImageSource = (string)entry["media$group"]["media$thumbnail"][0]["url"];
                        newent.Count = (int)entry["yt$countHint"]["$t"];
                        newent.Updated = (DateTime)entry["updated"]["$t"];
                        playlistentry.Add(newent);

                    }
                }
                catch (Exception ex)
                {
                    Resources.ErrorLogging.Log(this.GetType().ToString(),ex.Message, "PlaylistViewmodel",string.Empty);//ErrorLogging.Log(ex.Message + " response: " + e.API.Response);
                }
            });
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
                    string response_id = (string)apijson["feed"]["author"][0]["yt$userId"]["$t"];
                    if (ISOHelper.WriteToFile("cache/" + response_id + ".json", apijson.ToString()))
                    {
                        IsolatedStorageSettings.ApplicationSettings["cache"] = true;
                        IsolatedStorageSettings.ApplicationSettings["cache_filename"] = "cache/" + response_id + ".json";
                        IsolatedStorageSettings.ApplicationSettings.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Resources.ErrorLogging.Log(this.GetType().ToString(), ex.Message, "PlaylistViewmodel", string.Empty);
               
            }
            PopulateCollection(apijson);
        }

        public override void POST_Method_CallBack(IAsyncResult res)
        {
            throw new NotImplementedException();
        }
    }
}
