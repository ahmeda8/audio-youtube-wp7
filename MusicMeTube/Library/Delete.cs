using System;
using System.Net;
using System.IO.IsolatedStorage;
using System.IO;

namespace MusicMeTube
{
    public class Delete
    {
        public static bool Completed = false;

        public static void Video(string playlist_id, string video_id)
        {
            Completed = false;
            string url = "http://gdata.youtube.com/feeds/api/playlists/"+HttpUtility.UrlEncode(playlist_id)+"/"+HttpUtility.UrlEncode(video_id);
            HttpWebRequest wr = HttpWebRequest.CreateHttp(url);
            wr.ContentType = "application/atom+xml";
            wr.Headers[HttpRequestHeader.Authorization] = "Bearer " + IsolatedStorageSettings.ApplicationSettings["access_token"];
            wr.Headers["GData-Version"] = "2";
            wr.Headers["X-GData-Key"] = "key=" + Library.Authentication.DEVELOPER_KEY;
            wr.Method = "DELETE";
            wr.BeginGetResponse(res => {
                try
                {
                    HttpWebResponse webres = (HttpWebResponse)wr.EndGetResponse(res);
                    StreamReader sr = new StreamReader(webres.GetResponseStream());
                    string str = sr.ReadToEnd();
                    Completed = true;
                }

                catch (Exception e)
                {
                    Completed = true;
                    Resources.ErrorLogging.Log("MusifyMyTube.Delete", e.Message, "Delete video", wr.RequestUri.OriginalString);
                }
            },wr);
        }

        public static void Playlist(string playlist_id)
        {
            Completed = false;
            string url = "http://gdata.youtube.com/feeds/api/users/default/playlists/" + HttpUtility.UrlEncode(playlist_id);
            HttpWebRequest wr = HttpWebRequest.CreateHttp(url);
            wr.ContentType = "application/atom+xml";
            wr.Headers[HttpRequestHeader.Authorization] = "Bearer " + IsolatedStorageSettings.ApplicationSettings["access_token"];
            wr.Headers["GData-Version"] = "2";
            wr.Headers["X-GData-Key"] = "key=" + Library.Authentication.DEVELOPER_KEY;
            wr.Method = "DELETE";
            wr.BeginGetResponse(res =>
            {
                try
                {
                    HttpWebResponse webres = (HttpWebResponse)wr.EndGetResponse(res);
                    StreamReader sr = new StreamReader(webres.GetResponseStream());
                    string str = sr.ReadToEnd();
                    Completed = true;
                }
                catch (Exception e)
                {
                    Completed = true;
                    Resources.ErrorLogging.Log("MusifyMyTube.Delete", e.Message, "Delete Playlist",wr.RequestUri.OriginalString);
                }
            }, wr);
 
        }
    }
}
