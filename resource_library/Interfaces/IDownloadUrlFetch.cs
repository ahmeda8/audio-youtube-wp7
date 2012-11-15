using System;

namespace ResourceLibrary
{
    interface IDownloadUrlFetch
    {
        event ApiCompletedEventHandler Completed;
        event ApiCompletedEventHandler Failed;
        void StartFetch(string ytsrc);
    }
}
