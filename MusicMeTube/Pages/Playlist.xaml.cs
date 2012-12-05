using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using MusicMeTube;
using Microsoft.Phone.Tasks;
using ResourceLibrary;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;

namespace MusicMeTube.Pages
{
    public partial class Playlist : PhoneApplicationPage
    {

        BackgroundWorker loader_worker;
        ViewModelPlaylist vmp;
        ProgressIndicator progindicator;
        
        public Playlist()
        {
            InitializeComponent();
#if DEBUG
            //adDuplexControl.IsTest = true;
            ApplicationTitle.Text = "DEBUG musify myTube";
#endif
            loader_worker = new BackgroundWorker();
            progindicator = new ProgressIndicator();
            loader_worker.DoWork += new DoWorkEventHandler(loader_worker_DoWork);
            loader_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loader_worker_RunWorkerCompleted);
            Loaded += new RoutedEventHandler(Playlist_Loaded);
        }

        void Playlist_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.SetProgressIndicator(this, progindicator);
            loader_worker.RunWorkerAsync();
        }

        void loader_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DataContext = vmp;
            ToggleProgressBar("Done");
        }

        void loader_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ToggleProgressBar("Loading...");
            vmp = new ViewModelPlaylist();
            while (!vmp.Completed)
            {
                System.Threading.Thread.Sleep(3000);
            }
        }

        private void ToggleProgressBar(string message)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => {
                //performanceprogressbar.IsEnabled = !performanceprogressbar.IsEnabled;
                //performanceprogressbar.IsIndeterminate = !performanceprogressbar.IsIndeterminate;
                progindicator.IsIndeterminate = !progindicator.IsIndeterminate;
                progindicator.IsVisible = !progindicator.IsVisible;
                progindicator.Text = message;
            });
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                int index = ViewModelPlaylist.playlistentry.IndexOf((Entry)listBox1.SelectedItem);
                NavigationService.Navigate(new Uri("/Pages/TrackList.xaml?index=" + index, UriKind.Relative));
            }
            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            listBox1.SelectedIndex = -1;
            NavigationService.RemoveBackEntry();
            base.OnNavigatedTo(e);
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {

            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.Remove("cache_filename");
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.Remove("refresh_token");
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.Save();
            ISOHelper.DeleteDirectory("cache");
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void datastat_click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/StatsPage.xaml", UriKind.Relative));
        }

        private void Syncstat_click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/SyncStats.xaml", UriKind.Relative));
        }

        private void createmodify_click(object sender, EventArgs e)
        {
            MessageBox.Show("You will now be taken to youtube website to manage your playlists");
            WebBrowserTask wb = new WebBrowserTask();
            wb.Uri = new Uri("http://m.youtube.com", UriKind.Absolute);
            wb.Show();
        }

        private void settings_click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Settings.xaml", UriKind.Relative));
        }

        private void instr_click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Instructions.xaml", UriKind.Relative));
        }

        //private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        //{
        //    ErrorLogging.Log(this.GetType().ToString(),e.Error.Message,string.Empty,string.Empty);
        //}

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ISOHelper.DeleteDirectory("cache");
            if(!loader_worker.IsBusy)
                loader_worker.RunWorkerAsync();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/CreatePlaylist.xaml", UriKind.Relative));
        }

    }
}