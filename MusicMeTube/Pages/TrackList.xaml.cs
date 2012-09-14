using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.Xml;
using System.Windows;
using Resources;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;
using System.ComponentModel;
using System.Net;

namespace MusicMeTube
{
    public partial class TrackList : PhoneApplicationPage
    {
        Entry plentry;
        ViewModelTracklist viewmodel;
        ProgressIndicator proindicator;
        BackgroundWorker back_worker;
        BackgroundWorker search;
        BackgroundWorker addvideo_worker;
        BackgroundWorker delvideo_worker;
        BackgroundWorker delplay_worker;

        ProgressReporter progrss_report;
        SearchResults sr;
        int index;
       
        public TrackList()
        {
            InitializeComponent();
#if DEBUG
            adDuplexControl.IsTest = true;
#endif
            proindicator = new ProgressIndicator();
            SystemTray.SetProgressIndicator(this, proindicator);

            back_worker = new BackgroundWorker();
            search = new BackgroundWorker();
            addvideo_worker = new BackgroundWorker();
            delvideo_worker = new BackgroundWorker();
            delplay_worker = new BackgroundWorker();

            back_worker.DoWork += new DoWorkEventHandler(back_worker_DoWork);
            back_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(back_worker_RunWorkerCompleted);
            search.DoWork += new DoWorkEventHandler(search_DoWork);
            search.RunWorkerCompleted += new RunWorkerCompletedEventHandler(search_RunWorkerCompleted);
            addvideo_worker.DoWork += new DoWorkEventHandler(addvideo_worker_DoWork);
            addvideo_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(addvideo_worker_RunWorkerCompleted);
            delvideo_worker.DoWork += new DoWorkEventHandler(delvideo_worker_DoWork);
            delvideo_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delvideo_worker_RunWorkerCompleted);
            delplay_worker.DoWork += new DoWorkEventHandler(delplay_worker_DoWork);
            delplay_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delplay_worker_RunWorkerCompleted);
            progrss_report = new ProgressReporter();
        }

        void delplay_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ToggleProgressBar("Done");
            ISOHelper.DeleteFile("cache\\" + plentry.Id + ".json");
            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing || BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused)
                BackgroundAudioPlayer.Instance.Stop();
            if (!ISOHelper.DeleteDirectory(plentry.Id))
                MessageBox.Show("Some files were locked by audio player, or do not exist");
            ISOHelper.DeleteDirectory("cache");
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NavigationService.GoBack();
                });
        }

        void delplay_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ToggleProgressBar("Deleting playlist...");
            Delete.Playlist((string)e.Argument);
            while (!Delete.Completed)
            {
                System.Threading.Thread.Sleep(3000);
            }
        }

        void delvideo_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ToggleProgressBar("Done");
            ISOHelper.DeleteFile("cache\\" + plentry.Id + ".json");
            if(!back_worker.IsBusy)
                back_worker.RunWorkerAsync(index);
        }

        void delvideo_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ToggleProgressBar("Deleting Video...");
            Delete.Video(plentry.Id, (string)e.Argument);
            while (!Delete.Completed)
            {
                System.Threading.Thread.Sleep(3000);
            }
        }

        void addvideo_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ToggleProgressBar("Done");
            ISOHelper.DeleteFile("cache\\" + plentry.Id + ".json");
            if(!back_worker.IsBusy)
                back_worker.RunWorkerAsync(index);
            if (Add.ErrorOccured)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => {
                    MessageBox.Show("Error occured while adding video to playlist");
                });
            }
        }

        void addvideo_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ToggleProgressBar("Adding video...");
            Add.Video(plentry.Id, (string)e.Argument);
            while (!Add.Completed)
            {
                System.Threading.Thread.Sleep(3000);
            }
        }

        void search_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            searchResultslist.DataContext = sr;
            ToggleProgressBar("Done");
        }

        void search_DoWork(object sender, DoWorkEventArgs e)
        {
            ToggleProgressBar("Searching...");
            sr.Next();
            while (!sr.completed)
            {
                System.Threading.Thread.Sleep(3000);
            }
        }

        void back_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DataContext = viewmodel;
            ToggleProgressBar("Done");
        }

        void back_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ToggleProgressBar("Loading...");
            plentry = ViewModelPlaylist.playlistentry.ElementAt((int)e.Argument);
            viewmodel = new ViewModelTracklist(plentry);
            while (!viewmodel.completed)
            {
                System.Threading.Thread.Sleep(3000);
            }
            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string indexstr;
            NavigationContext.QueryString.TryGetValue("index", out indexstr);
            index = int.Parse(indexstr);
            if(!back_worker.IsBusy)
                back_worker.RunWorkerAsync(index);
            if (App.GlobalOfflineSync != null)
            {
                App.GlobalOfflineSync.Ready += new FileDownloadEvntHandler(GlobalOfflineSync_Ready);
                App.GlobalOfflineSync.SyncProgressChange += new FileDownloadEvntHandler(GlobalOfflineSync_SyncProgressChange);
            }
           
            base.OnNavigatedTo(e);
        }

        void GlobalOfflineSync_SyncProgressChange(object sender,FileDownloadEvntArgs e)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                proindicator.IsVisible = true;
                proindicator.Text = e.Message;
            });
        }

       

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            App.GlobalOfflineSync.Ready -= new FileDownloadEvntHandler(GlobalOfflineSync_Ready);
            App.GlobalOfflineSync.SyncProgressChange -= new FileDownloadEvntHandler(GlobalOfflineSync_SyncProgressChange);
            base.OnNavigatingFrom(e);
        }
 
        private int PreparePlaylistXML(bool Play)
        {
            int numoftrack = 0;
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication()) 
            {

                lock (iso)
                {
                    using (IsolatedStorageFileStream isf = iso.CreateFile("playlist.xml"))
                    {
                        XDocument xdoc = new XDocument(new XDeclaration("1.0", "utf8", "yes"));
                        xdoc.AddFirst(new XElement("playlist"));
                        XAttribute plname = new XAttribute("name", plentry.Id);
                        xdoc.Element("playlist").Add(plname);
                        if (Play)
                        {
                            XAttribute plstart = new XAttribute("start", 1);
                            xdoc.Element("playlist").Add(plstart);
                        }
                        else
                        {
                            XAttribute plstart = new XAttribute("start", 0);
                            xdoc.Element("playlist").Add(plstart);
                        }
                        int select = 0;
                        if (listBox1.SelectedIndex > -1)
                            select = listBox1.SelectedIndex;

                        for (int i = 0; i < viewmodel.tracklistentry.Count; i++)
                        {
                            if (select == viewmodel.tracklistentry.Count)
                                select = 0;
                            var e = viewmodel.tracklistentry[select];
                            string filename = plentry.Id + "\\" + e.Id + ".mp3";
                            if (iso.FileExists(filename))
                            {
                                XElement xel = new XElement("item");
                                XAttribute src = new XAttribute("source", filename);
                                XAttribute id = new XAttribute("id", e.Id);
                                XAttribute title = new XAttribute("title", e.Title);
                                xel.Add(src);
                                xel.Add(id);
                                xel.Add(title);
                                xdoc.Element("playlist").Add(xel);
                                numoftrack++;
                            }
                            select++;
                        }

                        xdoc.Save(isf);
                    }
                }
            }
            return numoftrack;
           
        }
        
        private void Syncplay_click(object sender, EventArgs e)
        {
            bool usecellular = false;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("use_cellular",out usecellular);

            if (!usecellular && (NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
            {
                PreparePlaylistXML(true);
                //Play(false);
                DoOfflineSync();
            }
            else if (usecellular)
            {
                PreparePlaylistXML(true);
                //Play(false);
                DoOfflineSync();
            }
            else
            {
                MessageBox.Show("Your settings prohibit use of celluar network for download.");
            }
        }

        

        private void stop_click(object sender, EventArgs e)
        {
            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing || BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused)
            {
                BackgroundAudioPlayer.Instance.Stop();
            }
            if (App.GlobalOfflineSync != null)
            {
                App.GlobalOfflineSync.Cancel();
            }
        }

        
        private void DoOfflineSync()
        {
            if (App.GlobalOfflineSync.SOURCES.Count > 0)
            {
                proindicator.IsVisible = true;
                proindicator.Text = "Download Failed. Already in progress";
                return;
            }
            //OfflineSyncExt.DestinationInfo dinfo = new FileDownloader.DestinationInfo();

            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!iso.DirectoryExists(plentry.Id))
                    iso.CreateDirectory(plentry.Id);
            }

            int select = 0;
            if (listBox1.SelectedIndex > -1)
                select = listBox1.SelectedIndex;
            for (int i = 0; i < viewmodel.tracklistentry.Count; i++)
            {
                if (select == viewmodel.tracklistentry.Count)
                    select = 0;
                var listEntry = viewmodel.tracklistentry[select];
                string filename = plentry.Id + "\\" + listEntry.Id + ".mp3";
                if (ISOHelper.FileExists(filename))
                {
                    select++;
                    continue;
                }
                //dinfo.Title = listEntry.Title;
                //dinfo.Source = listEntry.Source;
                //dinfo.Destination = filename;
                App.GlobalOfflineSync.SOURCES.Add(listEntry);
                select++;
            }
            
            try
            {
                if (App.GlobalOfflineSync.SOURCES.Count > 0 )
                {
                    App.GlobalOfflineSync.Cancelled = false;
                    App.GlobalOfflineSync.Next();
                }
                else
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        proindicator.IsVisible = true;
                        proindicator.Text = "Sync Completed.";
                    });
                }
               
            }
            catch (Exception e)
            {
                //ErrorLogging.Log(e.Message);
                ErrorLogging.Log(this.GetType().ToString(),e.Message,"TracklistSyncError",string.Empty);
            }
            
        }

        void GlobalOfflineSync_Ready(object sender,FileDownloadEvntArgs e)
        {
            OfflineSyncExt sync = (OfflineSyncExt)sender;
            sync.BACKGROUND_REQUEST.TransferProgressChanged += new EventHandler<BackgroundTransferEventArgs>(Request_TransferProgressChanged);
        }

        void Request_TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            ProgressReporter.ProgressInfo P = progrss_report.GetProgressInfo(e.Request);
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                proindicator.IsVisible = true;
                proindicator.Text = "Downloading... "+P.Title;
                proindicator.Value = P.FileProgress;
            });
            if(P.FileProgress > 0.90f && !back_worker.IsBusy)
                back_worker.RunWorkerAsync(index);
        }


        private void datastat_click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/StatsPage.xaml", UriKind.Relative));
        }

        private void createmodify_click(object sender, EventArgs e)
        {
            MessageBox.Show("You will now be taken to youtube website to manage your playlists");
            WebBrowserTask wb = new WebBrowserTask();
            wb.Uri = new Uri("http://m.youtube.com", UriKind.Absolute);
            wb.Show();
        }

        private void nosyncplay_click(object sender, EventArgs e)
        {
            if (PreparePlaylistXML(true) <= 0)
                MessageBox.Show("No tracks available offline, please select 'download all' from menu.");
            else
                Play(true);
        }


        private void Play(bool PlayDefault)
        {
            if(BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing || BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused)
                BackgroundAudioPlayer.Instance.Stop();
            BackgroundAudioPlayer.Instance.Volume = 1.0D;
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (plentry == null || viewmodel.tracklistentry == null || viewmodel.tracklistentry.Count < 1)
                    return;
                string filename;
                int index = 0;
                if (listBox1.SelectedIndex > -1)
                    index = listBox1.SelectedIndex;
                filename = plentry.Id + "\\" + viewmodel.tracklistentry[index].Id + ".mp3";
                if (iso.FileExists(filename))
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        proindicator.IsVisible = true;
                        proindicator.Text = "Playing...";
                    });
                    BackgroundAudioPlayer.Instance.Play();
                }
                else
                {
                    if (PlayDefault)
                    {
                        System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            proindicator.IsVisible = true;
                            proindicator.Text = "Playing only offline available tracks.";
                        });
                        BackgroundAudioPlayer.Instance.Play();
                    }
                }
            }
        }

        private void clearsync_click(object sender, EventArgs e)
        {
            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing || BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused)
                BackgroundAudioPlayer.Instance.Stop();
            if (!ISOHelper.DeleteDirectory(plentry.Id))
                MessageBox.Show("Some files were locked by audio player, or do not exist");
            if (!back_worker.IsBusy)
                back_worker.RunWorkerAsync(index);
        }

        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            ErrorLogging.Log(this.GetType().ToString(), e.Error.Message, string.Empty, string.Empty);
        }

        private void ToggleProgressBar(string message)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    proindicator.IsIndeterminate = !proindicator.IsIndeterminate;
                    proindicator.IsVisible = !proindicator.IsVisible;
                    proindicator.Text = message;
                });
        }

        private void search_click(object sender, RoutedEventArgs e)
        {
            string query = txtsearch.Text;
            sr = new SearchResults(query);
            if(!search.IsBusy)
                search.RunWorkerAsync();
        }

        private void searchbox_focus(object sender, RoutedEventArgs e)
        {
            txtsearch.Text = "";
        }

        private void add_click(object sender, EventArgs e)
        {
            add_appbar = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
            add_appbar.IsEnabled = false;
            Entry en = searchResultslist.SelectedItem as Entry;
            if (!addvideo_worker.IsBusy)
                addvideo_worker.RunWorkerAsync(en.Id);
            else
                MessageBox.Show("Please wait before adding another video.");
            
        }

        private void searchlistbox_selected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (searchResultslist.SelectedIndex > -1)
            {
                add_appbar = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                add_appbar.IsEnabled = true;
            }
        }

        private void delete_click(object sender, EventArgs e)
        {
             MessageBoxResult mr = MessageBox.Show("Are you sure to delete selected track?", "delete track", MessageBoxButton.OKCancel);
            if (mr == MessageBoxResult.OK)
            {
                Entry en = listBox1.SelectedItem as Entry;
                if(!delvideo_worker.IsBusy)
                    delvideo_worker.RunWorkerAsync(en.EntryID);
#if DEBUG
                delete_appbar = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                delete_appbar.IsEnabled = false;
#endif
            }
        }

        private void listbox1_selected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
#if DEBUG
                delete_appbar = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                delete_appbar.IsEnabled = true;
#endif
            }
        }

        private void delplaylist_click(object sender, EventArgs e)
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure to delete this playlist?", "delete playlist", MessageBoxButton.OKCancel);
            if (mr == MessageBoxResult.OK)
            {
                if(!delplay_worker.IsBusy)
                    delplay_worker.RunWorkerAsync(plentry.Id);
            }
        }
        
    }
}