using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.BackgroundTransfer;

namespace ResourceLibrary
{
    public class DownloadQueuing 
    {
        //constants

        private static DownloadQueuing StaticInstance = null;
        private Messaging Message;
        private Downloader DownloaderSingle;
        private bool Aborted = false;

        //lists

        private List<Entry> YoutubeSourceList;
        private List<BackgroundTransferRequest> ReadyList;
        private List<BackgroundTransferRequest> CurrentList;
        
        //events

        public event GenericEvntHandler Completed;
        public event GenericEvntHandler Ready;


        // factory

        public static DownloadQueuing GetInstance(Messaging MsgClass)
        {
            if (StaticInstance == null)
            {
                StaticInstance = new DownloadQueuing(MsgClass);
            }
            return StaticInstance;
        }

        //construct

        public DownloadQueuing(Messaging MsgClass)
        {
            Message = MsgClass;
            DownloaderSingle = new Downloader(Message);
            YoutubeSourceList = new List<Entry>();
            DownloaderSingle.Completed += DownloaderSingle_Completed;
            DownloaderSingle.Ready += DownloaderSingle_Ready;
            Aborted = false;
        }

        //event subscribers

        void DownloaderSingle_Completed(object sender)
        {
            if (!Aborted && YoutubeSourceList.Count > 0)
            {
                DownloaderSingle.Start(YoutubeSourceList.First());
                YoutubeSourceList.Remove(YoutubeSourceList.First()); // remove the element after it started
            }
            else if(Completed != null)
            {
                Completed(this);
            }
        }

        void DownloaderSingle_Ready(object sender)
        {
            ReadyList.Add((BackgroundTransferRequest)sender);
            if (Ready != null)
                Ready(sender);
        }

        //public helper functions

        public bool Start(List<Entry> DownloadList)
        {
            Aborted = false;
            if (YoutubeSourceList.Count == 0)
                return false;
            YoutubeSourceList.AddRange(DownloadList);
            bool SingleDownloadStarted = false;
            do
            {
                SingleDownloadStarted = DownloaderSingle.Start(YoutubeSourceList.First());
                YoutubeSourceList.Remove(YoutubeSourceList.First()); // remove the element after it started
            }
            while (!SingleDownloadStarted && YoutubeSourceList.Count > 0);

            return SingleDownloadStarted ;
        }

        public void Abort()
        {
            Aborted = true;
            YoutubeSourceList.Clear();
            ReadyList.Clear();
            foreach (BackgroundTransferRequest btxrq in BackgroundTransferService.Requests)
                BackgroundTransferService.Remove(btxrq);
            DownloaderSingle.Abort();
        }

        public BackgroundTransferRequest GetNextBTR()
        {
            BackgroundTransferRequest tmp = null;

            if (ReadyList.Count > 0)
            {
                tmp = ReadyList.First();
                ReadyList.Remove(ReadyList.First());
                return tmp;
            }
            return tmp;
        }

        public void AddCurrentBTR(BackgroundTransferRequest BTXRQ)
        {
            CurrentList.Add(BTXRQ);
        }

        public List<BackgroundTransferRequest> GetActiveBTR()
        {
            return CurrentList;
        }
    }
}
