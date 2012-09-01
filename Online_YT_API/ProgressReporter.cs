using System;
using Microsoft.Phone.BackgroundTransfer;


namespace Resources
{
    public class ProgressReporter
    {
        public struct ProgressInfo
        {
            public float FileProgress;
            public string Title;
            public float TotalProgress;
            public long BytesTransferredSinceLast;
            public float Speed;
        }

        ProgressInfo P;
        TimeSpan ts;
        DateTime last_time;
        long BytesTransferredLast = 0;

        public ProgressReporter()
        {
            P = new ProgressInfo();
            ts = new TimeSpan();
        }

        public ProgressInfo GetProgressInfo(BackgroundTransferRequest e)
        {
            P.FileProgress = (float)e.BytesReceived / (float)e.TotalBytesToReceive;
            string[] splitted_tag = e.Tag.Split('|');
            ts = DateTime.Now - last_time;
            P.Speed = (float)(e.BytesReceived - BytesTransferredLast) / (float)ts.TotalSeconds/1024;
            P.Title = splitted_tag[1];
            BytesTransferredLast = e.BytesReceived;
            last_time = DateTime.Now;
            return P;
        }
    }
}
