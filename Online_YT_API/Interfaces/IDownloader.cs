using System;
using System.Text;
using Microsoft.Phone.BackgroundTransfer;

namespace Resources
{
    interface IDownloader
    {
        event GenericEvntHandler Completed;
        void Start(Entry thisone);
        void Abort();
        Entry Current {get;set;}
        BackgroundTransferRequest BTR { get;}        
    }
}
