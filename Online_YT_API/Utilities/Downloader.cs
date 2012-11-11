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
        private BackgroundTransferRequest BTR;
        private bool Aborted = true;
        private Messaging Message;
        private MP3FileUrlFetch UrlFetcher;
        private Entry _Current;

        //events
        public event GenericEvntHandler Completed;

        //Construct 

        public Downloader(Messaging MsgClass)
        {
            Message = MsgClass;
            UrlFetcher = new MP3FileUrlFetch();
            UrlFetcher.Completed += UrlFetcher_Completed;
        }

        public void Start(Entry thisone)
        {
            Current = thisone;
            UrlFetcher.StartFetch(Current.Source);
            Message.SetMessage("Converting. "+Current.Title);
        }

        // event catcher

        void UrlFetcher_Completed(object sender, APICompletedEventArgs e)
        {
            Uri Dest = new Uri("shared/transfers/temp"+sender.GetHashCode()+".mp3", UriKind.Relative);
            Uri Src = new Uri(e.Response, UriKind.Absolute);
            BTR = new BackgroundTransferRequest(Src, Dest);
            BTR.Tag = Current.PlaylistID+"/"+Current.Id+".mp3";
            BackgroundTransferService.Add(BTR);
            BTR.TransferStatusChanged += BTR_TransferStatusChanged;
            Message.SetMessage("Downloading. " + Current.Title);
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
                        Message.SetMessage("Completed. "+Current.Title);
                    }
                    break;
            }
        }

        public void Abort()
        {
            if (BackgroundTransferService.Find(BTR.RequestId) != null)
            {
                BackgroundTransferService.Remove(BTR);
                Message.SetMessage("Aborted. " + Current.Title);
            }
        }


        private Entry Current
        {
            get
            {
                return _Current;
            }
            set
            {
                _Current = value;
            }
        }
    }
}
