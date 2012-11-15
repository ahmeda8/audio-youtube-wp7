using System;
using System.Text;
using Microsoft.Phone.BackgroundTransfer;

namespace ResourceLibrary
{
    interface IDownloader
    {
        event GenericEvntHandler Completed;
        event GenericEvntHandler Ready;
        bool Start(Entry thisone);
        void Abort();
        BackgroundTransferRequest BTR { get;}        
    }
}
