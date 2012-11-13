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
        BackgroundWorker dataloading_worker;
        BackgroundWorker search;
        BackgroundWorker addvideo_worker;
        BackgroundWorker delvideo_worker;
        BackgroundWorker delplay_worker;
        SearchResults sr;
        int index;
        List<Entry> ManipulationList;

#region Construct

        public TrackList()
        {
            InitializeComponent();

            ManipulationList = new List<Entry>();
            App.GlobalMessaging.Changed += Message_Changed;
            proindicator = new ProgressIndicator();
            SystemTray.SetProgressIndicator(this, proindicator);

            dataloading_worker = new BackgroundWorker();
            search = new BackgroundWorker();
            addvideo_worker = new BackgroundWorker();
            delvideo_worker = new BackgroundWorker();
            delplay_worker = new BackgroundWorker();

            dataloading_worker.DoWork += new DoWorkEventHandler(back_worker_DoWork);
            dataloading_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(back_worker_RunWorkerCompleted);
            search.DoWork += new DoWorkEventHandler(search_DoWork);
            search.RunWorkerCompleted += new RunWorkerCompletedEventHandler(search_RunWorkerCompleted);
            addvideo_worker.DoWork += new DoWorkEventHandler(addvideo_worker_DoWork);
            addvideo_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(addvideo_worker_RunWorkerCompleted);
            delvideo_worker.DoWork += new DoWorkEventHandler(delvideo_worker_DoWork);
            delvideo_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delvideo_worker_RunWorkerCompleted);
            delplay_worker.DoWork += new DoWorkEventHandler(delplay_worker_DoWork);
            delplay_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delplay_worker_RunWorkerCompleted);
            App.GlobalOfflineSync.Completed += GlobalOfflineSync_Completed;
            App.GlobalOfflineSync.Ready += GlobalOfflineSync_Ready;
        }


#endregion

#region Background workers

        void delplay_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            App.GlobalMessaging.SetMessage("Deleting playlist...");
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
            ProgressIndicatorVisible(false);
        }

        void delplay_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProgressIndicatorVisible(true);
            Delete.Playlist((string)e.Argument);
            while (!Delete.Completed)
            {
                System.Threading.Thread.Sleep(3000);
            }
        }

        void delvideo_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            App.GlobalMessaging.SetMessage("Deleting tracks...");
            ISOHelper.DeleteFile("cache\\" + plentry.Id + ".json");
            ManipulationList.Clear(); // clear the list for delete files
            if(!dataloading_worker.IsBusy)
                dataloading_worker.RunWorkerAsync(index);
            ProgressIndicatorVisible(false);
        }

        void delvideo_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProgressIndicatorVisible(true);
            foreach (Entry ent in ManipulationList)
            {
                ISOHelper.DeleteFile(ent.PlaylistID + "\\" + ent.Id + ".mp3");
                Delete.Video(ent.PlaylistID, ent.EntryID);
                while (!Delete.Completed)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        void addvideo_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            App.GlobalMessaging.SetMessage("Adding...");
            ISOHelper.DeleteFile("cache\\" + plentry.Id + ".json");
            if(!dataloading_worker.IsBusy)
                dataloading_worker.RunWorkerAsync(index);
            if (Add.ErrorOccured)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => {
                    MessageBox.Show("Error occured while adding video to playlist");
                });
            }
            ProgressIndicatorVisible(false);
        }

        void addvideo_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProgressIndicatorVisible(true);
            Add.Video(plentry.Id, (string)e.Argument);
            while (!Add.Completed)
            {
                System.Threading.Thread.Sleep(3000);
            }
        }

        void search_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            searchResultslist.DataContext = sr;
            ProgressIndicatorVisible(false);
        }

        void search_DoWork(object sender, DoWorkEventArgs e)
        {
            ProgressIndicatorVisible(true);
            sr.Next();
            while (!sr.completed)
            {
                System.Threading.Thread.Sleep(2000);
            }
        }

        void back_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            App.GlobalMessaging.SetMessage("");
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => {
                DataContext = viewmodel;
            });
            ProgressIndicatorVisible(false);
        }

        void back_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProgressIndicatorVisible(true);
            App.GlobalMessaging.SetMessage("Loading Data...");
            plentry = ViewModelPlaylist.playlistentry.ElementAt((int)e.Argument);
            viewmodel = new ViewModelTracklist(plentry);
            while (!viewmodel.completed)
            {
                System.Threading.Thread.Sleep(1000);
            }
            
        }
#endregion

#region Navigation Events

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string indexstr;
            NavigationContext.QueryString.TryGetValue("index", out indexstr);
            index = int.Parse(indexstr);
            if(!dataloading_worker.IsBusy)
                dataloading_worker.RunWorkerAsync(index);
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            App.GlobalOfflineSync.Completed -= GlobalOfflineSync_Completed;
            App.GlobalMessaging.Changed -= Message_Changed;
            base.OnNavigatingFrom(e);
        }
#endregion

#region Event subscribers

        void Message_Changed(object sender, APICompletedEventArgs e)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    proindicator.Text = e.Response;
                    proindicator.IsVisible = true;
                });
        }

        void GlobalOfflineSync_Completed(object sender)
        {
            if(!dataloading_worker.IsBusy)
                dataloading_worker.RunWorkerAsync(index);
        }


        void GlobalOfflineSync_Ready(object sender)
        {
            BackgroundTransferRequest BackgroundTransfer = (BackgroundTransferRequest)sender;
            BackgroundTransfer.TransferProgressChanged += BackgroundTransfer_TransferProgressChanged;
        }

        void BackgroundTransfer_TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => {
                proindicator.IsIndeterminate = false;
                proindicator.IsVisible = true;
                double val = ((double)e.Request.BytesReceived / (double)e.Request.TotalBytesToReceive);
                proindicator.Value = val;

            });
        }

#endregion

#region Helper Functions

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

                        foreach (Entry ent in viewmodel.tracklistentry)
                        {
                            string filename = ent.PlaylistID + "/" + ent.Id + ".mp3";
                            if (iso.FileExists(filename))
                            {
                                XElement xel = new XElement("item");
                                XAttribute src = new XAttribute("source", filename);
                                XAttribute id = new XAttribute("id", ent.Id);
                                XAttribute title = new XAttribute("title", ent.Title);
                                xel.Add(src);
                                xel.Add(id);
                                xel.Add(title);
                                xdoc.Element("playlist").Add(xel);
                                numoftrack++;
                            }
                        }
                        xdoc.Save(isf);
                    }
                }
            }
            return numoftrack;
           
        }

        private void ProgressIndicatorVisible(bool Visibility)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => {
                if (Visibility)
                {
                    proindicator.IsIndeterminate = true;
                    proindicator.IsVisible = true;
                }
                else
                {
                    proindicator.IsIndeterminate = false;
                    proindicator.IsVisible = false;
                }
            });
        }

#endregion

#region Events trigger handlers

        private void stop_click(object sender, EventArgs e)
        {
            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing || BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused)
            {
                BackgroundAudioPlayer.Instance.Stop();
            }
            if (App.GlobalOfflineSync != null)
            {
                App.GlobalOfflineSync.Abort();
            }
        }

        private void datastat_click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/StatsPage.xaml", UriKind.Relative));
        }

        private void play_click(object sender, EventArgs e)
        {
            if (PreparePlaylistXML(true) <= 0)
                App.GlobalMessaging.SetMessage("No offline tracks.");
            else if (BackgroundAudioPlayer.Instance.PlayerState != PlayState.Playing)
            {
                BackgroundAudioPlayer.Instance.Play();
                BackgroundAudioPlayer.Instance.Volume = 1.0;
            }
        }

        private void search_click(object sender, RoutedEventArgs e)
        {
            string query = txtsearch.Text;
            sr = new SearchResults(query);
            App.GlobalMessaging.SetMessage("Searching...");
            if (!search.IsBusy)
                search.RunWorkerAsync();
        }

        private void searchbox_focus(object sender, RoutedEventArgs e)
        {
            txtsearch.Text = "";
        }

        private void add_click(object sender, EventArgs e)
        {
            App.GlobalMessaging.SetMessage("Adding...");
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
            if (listBox1.SelectedItems.Count == 0)
                listBox1.IsSelectionEnabled = true;
            else
            {
                MessageBoxResult mr = MessageBox.Show("Are you sure to delete selected track?", "delete track", MessageBoxButton.OKCancel);
                if (mr == MessageBoxResult.OK)
                {
                    foreach (Entry ent in listBox1.SelectedItems)
                    {
                        ManipulationList.Add(ent);
                    }
                    if (!delvideo_worker.IsBusy)
                        delvideo_worker.RunWorkerAsync();
                }
            }
        }

        private void delplaylist_click(object sender, EventArgs e)
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure to delete this playlist?", "delete playlist", MessageBoxButton.OKCancel);
            if (mr == MessageBoxResult.OK)
            {
                App.GlobalMessaging.SetMessage("Deleting selected videos.");
                if (!delplay_worker.IsBusy)
                    delplay_worker.RunWorkerAsync(plentry.Id);
            }
        }

        private void Download_click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                if (App.GlobalOfflineSync.ManipulationList.Count == 0)
                {
                    List<Entry> tempList = new List<Entry>();
                    tempList.Clear();
                    foreach (Entry ent in listBox1.SelectedItems)
                    {
                        if(!ISOHelper.FileExists(ent.PlaylistID+"/"+ent.Id+".mp3"))
                            tempList.Add(ent);
                    }
                    if(tempList.Count > 0)
                        App.GlobalOfflineSync.Start(tempList);
                }
                else
                {
                    App.GlobalMessaging.SetMessage("Previous download in progress.");
                }
            }
            else if(!listBox1.IsSelectionEnabled)
            {
                listBox1.IsSelectionEnabled = true;
            }
        }

        private void ListItem_tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!listBox1.IsSelectionEnabled)
                listBox1.IsSelectionEnabled = true;
            else if (listBox1.SelectedItems.Count == 0)
                listBox1.IsSelectionEnabled = false;
        }
#endregion

    }
}