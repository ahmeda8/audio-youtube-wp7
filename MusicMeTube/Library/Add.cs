using System;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace MusicMeTube
{
    public class Add
    {
        public static bool Completed = false;
        public static bool ErrorOccured = false;

        public static void Video(string playlist_id, string video_id)
        {
            Completed = false;
            string url = "http://gdata.youtube.com/feeds/api/playlists/"+playlist_id;
            string message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"+
                            "<entry xmlns=\"http://www.w3.org/2005/Atom\" "+
                            "xmlns:yt=\"http://gdata.youtube.com/schemas/2007\">"+
                            "<id>"+video_id+"</id>"+
                            "</entry>";
            HttpWebRequest wr = HttpWebRequest.CreateHttp(url);
            wr.Method = "POST";
            wr.ContentType = "application/atom+xml";
            wr.Headers[HttpRequestHeader.ContentLength] = message.Length.ToString();
            wr.Headers[HttpRequestHeader.Authorization] = "OAuth " + IsolatedStorageSettings.ApplicationSettings["access_token"];
            wr.Headers["GData-Version"] = "2";
            wr.Headers["X-GData-Key"] = "key="+Library.Authentication.DEVELOPER_KEY;

            wr.BeginGetRequestStream(result => {
                Stream post_stream = wr.EndGetRequestStream(result);
                byte[] ba = Encoding.UTF8.GetBytes(message);
                post_stream.Write(ba, 0, ba.Length);
                post_stream.Close();
                try
                {
                    wr.BeginGetResponse(res =>
                    {
                        HttpWebResponse webres = (HttpWebResponse)wr.EndGetResponse(res);
                        StreamReader sr = new StreamReader(webres.GetResponseStream());
                        string str = sr.ReadToEnd();
                        Completed = true;
                    }, wr);
                }
                catch (Exception ext)
                {
                    Completed = true;  
                }
            },wr);
        }

        public static void Playlist(string name , string desc)
        {
            Completed = false;
            ErrorOccured = false;
            string url = "http://gdata.youtube.com/feeds/api/users/default/playlists";
            string message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<entry xmlns=\"http://www.w3.org/2005/Atom\" " +
                            "xmlns:yt=\"http://gdata.youtube.com/schemas/2007\">" +
                            "<title type=\"text\">" + name + "</title>" +
                            "<summary>" + desc + "</summary>" +
                            "</entry>";
            HttpWebRequest wr = HttpWebRequest.CreateHttp(url);
            wr.Method = "POST";
            wr.ContentType = "application/atom+xml";
            wr.Headers[HttpRequestHeader.ContentLength] = message.Length.ToString();
            wr.Headers[HttpRequestHeader.Authorization] = "Bearer " + IsolatedStorageSettings.ApplicationSettings["access_token"];
            wr.Headers["GData-Version"] = "2";
            wr.Headers["X-GData-Key"] = "key=" + Library.Authentication.DEVELOPER_KEY;

            wr.BeginGetRequestStream(result =>
            {
                Stream post_stream = wr.EndGetRequestStream(result);
                byte[] ba = Encoding.UTF8.GetBytes(message);
                post_stream.Write(ba, 0, ba.Length);
                post_stream.Close();
                try
                {
                    wr.BeginGetResponse(res =>
                    {
                        HttpWebResponse webres = (HttpWebResponse)wr.EndGetResponse(res);
                        StreamReader sr = new StreamReader(webres.GetResponseStream());
                        //string str = sr.ReadToEnd();
                        Completed = true;
                    }, wr);
                }
                catch (Exception ext)
                {
                    Completed = true;
                    ErrorOccured = true;
                }
            }, wr);
        }
        
    }
}
