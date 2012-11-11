using System;
using System.Text;
using Microsoft.Phone.BackgroundTransfer;

namespace Resources
{
    interface IDownloader
    {
        event GenericEvntHandler Completed;
        event GenericEvntHandler Ready;
        void Start(Entry thisone);
        void Abort();
        BackgroundTransferRequest BTR { get;}        
    }
}
