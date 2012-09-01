using System;

namespace Resources
{
    interface IDownloadUrlFetch
    {
        event ApiCompletedEventHandler Completed;
        event ApiCompletedEventHandler Failed;
        void StartFetch(string ytsrc);
    }
}
