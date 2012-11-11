using System;
using System.Net;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Collections.Generic;

namespace Resources
{
    public class OfflineSyncExt : FileDownloader ,ISyncing
    {
        private MP3FileUrlFetch url_fetcher;
        public BackgroundTransferRequest BACKGROUND_REQUEST;
        public Messaging Message;
        private string tag; //format dst_filename | title
        public bool Cancelled = false;
        public List<Entry> SOURCES { get; set; }

        public OfflineSyncExt()
        {
            url_fetcher = new MP3FileUrlFetch();
            url_fetcher.Completed += new ApiCompletedEventHandler(url_fetcher_Completed);
            url_fetcher.Failed += new ApiCompletedEventHandler(url_fetcher_Failed);
            tag = "";
            SOURCES = new List<Entry>();
        }

        void url_fetcher_Failed(object sender, APICompletedEventArgs e)
        {
            string[] title = tag.Split('|');
           // RaiseSyncChange(new FileDownloadEvntArgs("Request Failed... "+title[1]));
            Message.SetMessage("Request Failed... "+title[1]);
            RaiseFailed(new FileDownloadEvntArgs(e.Response));
        }

        void url_fetcher_Completed(object sender, APICompletedEventArgs e)
        {
            BACKGROUND_REQUEST = base.GetBackTXRequest(e.Response,tag);
            BACKGROUND_REQUEST.TransferStatusChanged += new EventHandler<BackgroundTransferEventArgs>(current_request_TransferStatusChanged);
            RaiseReady(new FileDownloadEvntArgs(e.Response));
            string[] title = BACKGROUND_REQUEST.Tag.Split('|');
            if(SOURCES.Count > 0)
                //RaiseSyncChange(new FileDownloadEvntArgs("Downloading... "+title[1]));
                Message.SetMessage("Downloading... "+title[1]);
            else
                //RaiseSyncChange(new FileDownloadEvntArgs("Completed"));
                Message.SetMessage("Completed");
        }

        void current_request_TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            switch (e.Request.TransferStatus)
            {
                case TransferStatus.Completed:
                    if (e.Request.StatusCode != 0)
                    {
                        string[] filename = e.Request.Tag.Split('|');
                        ISOHelper.MoveFileOverwrite(e.Request.DownloadLocation.OriginalString, filename[0]);
                        BackgroundTransferService.Remove(e.Request);
                    }
                    long totaldata = 0;
                    long session_data = 0;

                    IsolatedStorageSettings.ApplicationSettings.TryGetValue("total_data", out totaldata);
                    totaldata += e.Request.BytesReceived / 1024 / 1024;
                    IsolatedStorageSettings.ApplicationSettings["total_data"] = totaldata;
                    IsolatedStorageSettings.ApplicationSettings.TryGetValue("session_data", out session_data);
                    session_data += e.Request.BytesReceived / 1024 / 1024;
                    IsolatedStorageSettings.ApplicationSettings["session_data"] = session_data;
                    IsolatedStorageSettings.ApplicationSettings.Save();
                    if(!Cancelled)
                        //RaiseCompleted(new FileDownloadEvntArgs("Completed"));
                        Message.SetMessage("Downloading complete");
                    if (SOURCES.Count == 0)
                        //RaiseSyncChange(new FileDownloadEvntArgs("Completed"));
                        Message.SetMessage("Downloading complete");
                    break;
            }
        }

        public override void Next()
        {
            if (!Cancelled && SOURCES.Count > 0)
            {
                Entry current_entry = SOURCES.FirstOrDefault();
                tag = current_entry.PlaylistID + "\\" + current_entry.Id + ".mp3" + "|" + current_entry.Title;
                SOURCES.Remove(SOURCES.FirstOrDefault());
                //RaiseSyncChange(new FileDownloadEvntArgs("Requesting... " + current_entry.Title));
                Message.SetMessage("Requesting... " + current_entry.Title);
                url_fetcher.StartFetch(current_entry.Source);
            }
        }

        public override void Cancel()
        {
            try
            {
                url_fetcher.Dispose();
                if (BACKGROUND_REQUEST != null && !Cancelled)
                {
                    BackgroundTransferService.Remove(BACKGROUND_REQUEST);
                    BACKGROUND_REQUEST = null;
                    SOURCES.Clear();
                }
                else if (SOURCES != null && SOURCES.Count > 0)
                {
                    SOURCES.Clear();
                }
                Cancelled = true;
                //RaiseSyncChange(new FileDownloadEvntArgs("Syncing Cancelled"));
                Message.SetMessage("Downloading cancelled.");
            }
            catch (Exception e)
            {
                ErrorLogging.Log(this.GetType().ToString(), e.Message, "OfflineSyncCancelException", string.Empty);
            }
        }

        public void CancellAll()
        {
            //RaiseSyncChange(new FileDownloadEvntArgs("Syncing Cancelled"));
            foreach (var t in BackgroundTransferService.Requests)
                BackgroundTransferService.Remove(t);
            Cancelled = true;
            Message.SetMessage("Downloading cancelled.");
        }
    }
}
