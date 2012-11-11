using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Phone.Controls;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;
using Resources;

namespace MusicMeTube.Pages
{
    public partial class StatsPage : PhoneApplicationPage
    {
        DateTime last_ti = DateTime.Now;
        ProgressReporter p_reporter;
        BackgroundTransferRequest req;

        public StatsPage()
        {
            InitializeComponent();
            Loaded += new System.Windows.RoutedEventHandler(StatsPage_Loaded);
            p_reporter = new ProgressReporter();
            req = null;
        }

        void StatsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateUsageData();
            if (App.GlobalOfflineSync != null)
            {
                App.GlobalOfflineSync.Ready += new FileDownloadEvntHandler(GlobalOfflineSync_Ready);
                App.GlobalOfflineSync.SyncProgressChange += new FileDownloadEvntHandler(GlobalOfflineSync_SyncProgressChange);
                req = App.GlobalOfflineSync.BACKGROUND_REQUEST;//BackgroundTransferService.Requests.FirstOrDefault();
                if(req != null)
                    req.TransferProgressChanged += new EventHandler<BackgroundTransferEventArgs>(StatsPage_TransferProgressChanged);
                progressbar.Maximum = 1;
                progressbar.Minimum = 0;
                progressbar.Value = 0;
            }
            else
                title.Text = "No Syncing in Progress";
        }

        void GlobalOfflineSync_SyncProgressChange(object sender,FileDownloadEvntArgs e)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    title.Text = e.Message;
                });
        }

        void GlobalOfflineSync_Ready(object sender, FileDownloadEvntArgs e)
        {
            OfflineSyncExt sync = (OfflineSyncExt)sender;
            sync.BACKGROUND_REQUEST.TransferProgressChanged += new EventHandler<BackgroundTransferEventArgs>(StatsPage_TransferProgressChanged);
        }

        void StatsPage_TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            ProgressReporter.ProgressInfo pinfo = p_reporter.GetProgressInfo(e.Request);
            title.Text = "Downloading... "+pinfo.Title;
            progressbar.Value = pinfo.FileProgress;
            speedtxt.Text = pinfo.Speed + " Kb/s";
            last_ti = DateTime.Now;
            UpdateUsageData();
        }

        private void UpdateUsageData()
        {
            long avstor = IsolatedStorageFile.GetUserStoreForApplication().AvailableFreeSpace / 1024 / 1024;
            long totald = 0;
            long sessiond = 0;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("total_data", out totald);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("session_data", out sessiond);
            totald = totald + (long)0.3 * totald;
            sessiond = sessiond + (long)0.3 * sessiond;
            avstorage.Text = avstor.ToString();
            totaldata.Text = totald.ToString();
            sessiondata.Text = sessiond.ToString();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            App.GlobalOfflineSync.Ready -= new FileDownloadEvntHandler(GlobalOfflineSync_Ready);
            App.GlobalOfflineSync.SyncProgressChange -= new FileDownloadEvntHandler(GlobalOfflineSync_SyncProgressChange);
            if(req != null)
                req.TransferProgressChanged -= new EventHandler<BackgroundTransferEventArgs>(StatsPage_TransferProgressChanged);
            base.OnNavigatedFrom(e);
        }

        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            ErrorLogging.Log(this.GetType().ToString(), e.Error.Message, string.Empty, string.Empty);
        }

        private void stop_click(object sender, EventArgs e)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                App.GlobalOfflineSync.CancellAll();
                progressbar.Value = 0;
            });
        }
    }
}