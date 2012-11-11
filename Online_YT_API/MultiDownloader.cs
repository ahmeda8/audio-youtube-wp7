using System;
using System.Collections.Generic;
using System.Linq;

namespace Resources
{
    public class MultiDownloader
    {
        //constants

        private Messaging Message;
        private Downloader DownloaderSingle;
        public List<Entry> ManipulationList;
        private bool Aborted = false;

        //events

        public event GenericEvntHandler Completed;
        public event GenericEvntHandler Ready;

        //construct

        public MultiDownloader(Messaging MsgClass)
        {
            Message = MsgClass;
            DownloaderSingle = new Downloader(Message);
            ManipulationList = new List<Entry>();
            DownloaderSingle.Completed += DownloaderSingle_Completed;
            DownloaderSingle.Ready += DownloaderSingle_Ready;
            Aborted = false;
        }

       

        //event subscribers

        void DownloaderSingle_Completed(object sender)
        {
            if (!Aborted && ManipulationList.Count > 0)
            {
                DownloaderSingle.Start(ManipulationList.First());
                ManipulationList.Remove(ManipulationList.First()); // remove the element after it started
            }
            else if(Completed != null)
            {
                Completed(this);
            }
        }

        void DownloaderSingle_Ready(object sender)
        {
            if (Ready != null)
                Ready(sender);
        }
        //public helper functions

        public void Start(List<Entry> DownloadList)
        {
            Aborted = false;
            ManipulationList.Clear();
            ManipulationList = DownloadList;
            DownloaderSingle.Start(ManipulationList.First());
            ManipulationList.Remove(ManipulationList.First()); // remove the element after it started
        }

        public void Abort()
        {
            Aborted = true;
            ManipulationList.Clear();
            DownloaderSingle.Abort();
        }
       

    }
}
