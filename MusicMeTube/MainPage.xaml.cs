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
        ApplicationBarIconButton loginbtn;
        ProgressIndicator progindicator;

        public MainPage()
        {
            InitializeComponent();
            IsolatedStorageSettings.ApplicationSettings["session_data"] = 0L;
            IsolatedStorageSettings.ApplicationSettings.Save();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
            loginbtn = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
            progindicator = new ProgressIndicator();
            SystemTray.SetProgressIndicator(this, progindicator);
            if (Network.IsConnected())
            {   
                auth = new Authentication();
                auth.NewTokenReceived += new TokenReceivedEventHandler(auth_NewTokenReceived);
            }
            
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleProgressBar("Loading...");
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
                ToggleProgressBar("No connectivity");
            }
        }

        void auth_NewTokenReceived(object sender, APICompletedEventArgs e)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ToggleProgressBar("Done");
                NavigationService.Navigate(new Uri("/Pages/Playlist.xaml", UriKind.Relative));
            });
        }

        private void navigated_event(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Uri navigateduri = e.Uri;
            var cntnt = webBrowser1.Source;
            ToggleProgressBar("");
            loginbtn.IsEnabled = false;
        }

        private void navigationfailed_event(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            Uri navigateduri = e.Uri;
            ToggleProgressBar("");
            
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
                    MessageBox.Show("Access Denied, please select login from menu to try again.");
                    loginbtn.IsEnabled = true;
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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            NavigationService.RemoveBackEntry();
            base.OnNavigatedTo(e);
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            navigate_clear = false;
            NavigationService.Navigate(new Uri("/Pages/Instructions.xaml", UriKind.Relative));    
        }

        private void ToggleProgressBar(string message)
        {
            //performanceprogressbar.IsEnabled = !performanceprogressbar.IsEnabled;
            //performanceprogressbar.IsIndeterminate = !performanceprogressbar.IsIndeterminate;
            progindicator.IsVisible = !progindicator.IsVisible;
            progindicator.IsIndeterminate = !progindicator.IsIndeterminate;
            progindicator.Text = message;
        }

        private void login_click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(new Uri(auth.getAuthURI(), UriKind.Absolute));
        }

    }
}