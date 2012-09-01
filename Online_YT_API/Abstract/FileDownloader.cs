using System;
using System.Net;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;

namespace Resources
{
    
    
    public abstract class FileDownloader
    {
        public event FileDownloadEvntHandler Completed;
        public event FileDownloadEvntHandler Failed;
        public event FileDownloadEvntHandler Ready;
        public event FileDownloadEvntHandler SyncProgressChange;

        protected BackgroundTransferRequest GetBackTXRequest(string src,string tag)
        {
            Uri src_uri = new Uri(src, UriKind.Absolute);
            Uri dst_uri = new Uri("shared\\transfers\\temp"+src_uri.GetHashCode()+".mp3", UriKind.Relative);
            BackgroundTransferRequest back_req = new BackgroundTransferRequest(src_uri, dst_uri);
            back_req.Tag = tag;
            bool use_cellular = false;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("use_cellular", out use_cellular);
            if (use_cellular)
                back_req.TransferPreferences = TransferPreferences.AllowCellularAndBattery;
            else
                back_req.TransferPreferences = TransferPreferences.AllowBattery;
            return back_req;
        }

        protected virtual void RaiseCompleted(FileDownloadEvntArgs e)
        {
            if (Completed != null)
                Completed(this,e);
        }

        protected virtual void RaiseFailed(FileDownloadEvntArgs e)
        {
            ErrorLogging.Log(this.GetType().ToString(), e.Message, "FileDownloaderError", string.Empty);
            if (Failed != null)
                Failed(this,e);
        }

        protected virtual void RaiseReady(FileDownloadEvntArgs e)
        {
            if (Ready != null)
                Ready(this,e);
        }

        protected virtual void RaiseSyncChange(FileDownloadEvntArgs e)
        {
            if (SyncProgressChange != null)
                SyncProgressChange(this,e);
        }

        public abstract void Next();
        public abstract void Cancel();
    }
}
