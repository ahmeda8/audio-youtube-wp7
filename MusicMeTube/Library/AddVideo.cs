using System;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace MusicMeTube
{
    public class AddVideo
    {
        public static void Add(string playlist_id, string video_id)
        {
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
                    }, wr);
                }
                catch (Exception ext)
                {
                    
                }
            },wr);
        }
        
    }
}
