using System;
using System.Windows;
using Microsoft.Phone.Controls;
using MusicMeTube.Library;
using System.IO.IsolatedStorage;
using Resources;
using Microsoft.Phone.Shell;

namespace MusicMeTube
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor

        Authentication auth;
        bool navigate_clear = true;
#if DEBUG
        RTSP.RTSP_FileDownloader fd = new RTSP.RTSP_FileDownloader();
#endif
        
        public MainPage()
        {
            InitializeComponent();
            IsolatedStorageSettings.ApplicationSettings["session_data"] = 0L;
            IsolatedStorageSettings.ApplicationSettings.Save();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
            if (Network.IsConnected())
            {   
                auth = new Authentication();
                auth.NewTokenReceived += new TokenReceivedEventHandler(auth_NewTokenReceived);
            }
            
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            string filename;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("cache_filename", out filename);

            if (Network.IsConnected())
            {
                TimeSpan token_validity;
                DateTime request_time;
                int token_expires_in = 0;
                string refresh_token;
                IsolatedStorageSettings.ApplicationSettings.TryGetValue("request_time", out request_time);
                IsolatedStorageSettings.ApplicationSettings.TryGetValue("expires_in", out token_expires_in);
                IsolatedStorageSettings.ApplicationSettings.TryGetValue("refresh_token", out refresh_token);
                token_validity = DateTime.Now - request_time;
                navigate_clear = true;
                if (refresh_token !=null && token_validity.TotalSeconds < token_expires_in )
                {
                    auth.RefreshToken();
                }
                else if (refresh_token != null)
                {
                    auth.RefreshToken();
                }
                else
                {
                    webBrowser1.Navigate(new Uri(auth.getAuthURI(), UriKind.Absolute));
                }
            }
            else if (ISOHelper.FileExists(filename))
            {
                NavigationService.Navigate(new Uri("/Pages/Playlist.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show("No connectivity to internet and no cache available");
            }
#if DEBUG
            fd.GetRTPSocketPort();
#endif
        }

        void auth_NewTokenReceived(object sender, APICompletedEventArgs e)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/Pages/Playlist.xaml", UriKind.Relative));
            });
        }

        private void navigated_event(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Uri navigateduri = e.Uri;
            var cntnt = webBrowser1.Source;
        }

        private void navigationfailed_event(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            Uri navigateduri = e.Uri;
        }

        private void navigating_event(object sender, NavigatingEventArgs e)
        {
            if (e.Uri.Host.Equals("localhost"))
            {
                e.Cancel = true;
                string code = e.Uri.Query;
                string[] split_response_query = e.Uri.Query.Split('=');
                if (split_response_query[0] == "?code")
                {
                    auth.AuthCode = split_response_query[1];
                    auth.GetAccessTokenFromGoogle();
                }
                else
                {
                    MessageBox.Show("Authentication Access Denied");
                    webBrowser1.Navigate(new Uri(auth.getAuthURI(), UriKind.Absolute));
                }
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (navigate_clear)
            {
                NavigationService.RemoveBackEntry();
                base.OnNavigatedFrom(e);
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            navigate_clear = false;
            NavigationService.Navigate(new Uri("/Pages/Instructions.xaml", UriKind.Relative));    
        }

    }
}