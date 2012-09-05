using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using MusicMeTube;
using Microsoft.Phone.Tasks;
using Resources;
using System.IO.IsolatedStorage;

namespace MusicMeTube.Pages
{
    public partial class Playlist : PhoneApplicationPage
    {
        
        public Playlist()
        {
            InitializeComponent();
            ViewModelPlaylist vmp = new ViewModelPlaylist();
            DataContext = vmp;
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

        private void listBox1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            performanceprogressbar.IsEnabled = false;
            performanceprogressbar.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void instr_click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Instructions.xaml", UriKind.Relative));
        }

        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            ErrorLogging.Log(this.GetType().ToString(),e.Error.Message,string.Empty,string.Empty);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ISOHelper.DeleteDirectory("cache");
            ViewModelPlaylist vmp = new ViewModelPlaylist();
            this.DataContext = vmp;
        }

        private void Add_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/CreatePlaylist.xaml", UriKind.Relative));
        }

    }
}