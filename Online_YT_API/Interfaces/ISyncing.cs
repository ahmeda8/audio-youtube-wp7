using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Resources
{
   public interface ISyncing
    {
        List<Entry> SOURCES { get; set; }
        Messaging Message;
        void Next();
        void Cancel();
        void CancellAll();
        event FileDownloadEvntHandler Completed;
        event FileDownloadEvntHandler Failed;
        event FileDownloadEvntHandler Ready;
        event FileDownloadEvntHandler SyncProgressChange;
        
    }
}
