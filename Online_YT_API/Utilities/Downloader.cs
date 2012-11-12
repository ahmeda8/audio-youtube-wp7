using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.BackgroundTransfer;

namespace Resources
{
    public class Downloader : IDownloader
    {
        // constants
        private BackgroundTransferRequest _BTR;
        private bool Aborted = false;
        private Messaging Message;
        private MP3FileUrlFetch UrlFetcher;
        private Entry _Current;

        //events
        public event GenericEvntHandler Completed;
        public event GenericEvntHandler Ready;

        //Construct 

        public Downloader(Messaging MsgClass)
        {
            Message = MsgClass;
            UrlFetcher = new MP3FileUrlFetch();
            UrlFetcher.Completed += UrlFetcher_Completed;
            UrlFetcher.Failed += UrlFetcher_Failed;
        }

        //implement interface

        public void Start(Entry thisone)
        {
            Aborted = false;
            _Current = thisone;
            UrlFetcher.StartFetch(_Current.Source);
            Message.SetMessage("Converting - "+_Current.Title);
        }

        public void Abort()
        {
            if (_BTR != null && BackgroundTransferService.Find(_BTR.RequestId) != null)
            {
                BackgroundTransferService.Remove(_BTR);
                Message.SetMessage("Aborted - " + _Current.Title);
            }
            Aborted = true;
            RaiseCompleted();
        }

        public BackgroundTransferRequest BTR
        {
            get { return _BTR; }
        }

        // event catcher

        void UrlFetcher_Completed(object sender, APICompletedEventArgs e)
        {
            if (!Aborted)
            {
                Uri Dest = new Uri("shared/transfers/temp" + sender.GetHashCode() + ".mp3", UriKind.Relative);
                Uri Src = new Uri(e.Response, UriKind.Absolute);
                _BTR = new BackgroundTransferRequest(Src, Dest);
                _BTR.Tag = _Current.PlaylistID + "/" + _Current.Id + ".mp3";
                _BTR.TransferStatusChanged += BTR_TransferStatusChanged;
                Message.SetMessage("Downloading - " + _Current.Title);
                if (Ready != null)
                    Ready(_BTR);
            }
        }

        void UrlFetcher_Failed(object sender, APICompletedEventArgs e)
        {
            Message.SetMessage("Conversion failed - "+_Current.Title);
            RaiseCompleted();
        }

        void BTR_TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            switch (e.Request.TransferStatus)
            {
                case TransferStatus.Completed :
                    if (e.Request.StatusCode != 0)
                    {
                        ISOHelper.MoveFileOverwrite(e.Request.DownloadLocation.OriginalString, e.Request.Tag);
                        BackgroundTransferService.Remove(e.Request);
                        Message.SetMessage("Completed - "+_Current.Title);
                        ISOHelper.SaveDataUsage(e.Request.BytesReceived);
                        RaiseCompleted();
                    }
                    break;
            }
        }

        //helper function 

        private void RaiseCompleted()
        {
            if (Completed != null)
                Completed(this);
        }
    }
}
