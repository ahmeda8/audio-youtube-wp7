using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Resources;

namespace MusicMeTube.Pages
{
    public partial class CreatePlaylist : PhoneApplicationPage
    {
        public CreatePlaylist()
        {
            InitializeComponent();
        }

        private void save_click(object sender, EventArgs e)
        {
            string name = txtName.Text;
            string desc = txtDesc.Text;
            string url = "http://gdata.youtube.com/feeds/api/users/default/playlists";
            string message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"+
                            "<entry xmlns=\"http://www.w3.org/2005/Atom\" "+
                            "xmlns:yt=\"http://gdata.youtube.com/schemas/2007\">"+
                            "<title type=\"text\">"+name+"</title>"+
                            "<summary>"+desc+"</summary>"+
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
                        System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("Playlist added, Please refresh playlist page to see newly added playlist");
                            if (NavigationService.CanGoBack)
                                NavigationService.GoBack();
                        });
                    }, wr);
                }
                catch (Exception ext)
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        { 
                            MessageBox.Show("Error occured while adding playlist");
                        });
                }
            },wr);
        }
    }
}