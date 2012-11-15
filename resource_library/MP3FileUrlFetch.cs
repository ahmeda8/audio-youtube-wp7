using System;

namespace ResourceLibrary
{
    public class MP3FileUrlFetch:IDownloadUrlFetch,IDisposable
    {
        public event ApiCompletedEventHandler Completed;

        public event ApiCompletedEventHandler Failed;

        private youtube_org primary_fetch;
        private simplemp3_net secondary_fetch;
        private video2mp3_at tertiary_fetch;
        private string youtube_source;

        private bool Disposed = false;

        protected virtual void RaiseCompleted(APICompletedEventArgs e)
        {
            if (Completed != null && !Disposed)
                Completed(this, e);
        }

        protected virtual void RaiseFailed(APICompletedEventArgs e)
        {
            if (Failed != null && !Disposed)
                Failed(this, e);
        }

        public MP3FileUrlFetch()
        {
            primary_fetch = new youtube_org();
            secondary_fetch = new simplemp3_net();
            tertiary_fetch = new video2mp3_at();
            primary_fetch.Completed += new ApiCompletedEventHandler(primary_fetch_Completed);
            primary_fetch.Failed += new ApiCompletedEventHandler(primary_fetch_Failed);
            secondary_fetch.Completed += new ApiCompletedEventHandler(secondary_fetch_Completed);
            secondary_fetch.Failed += new ApiCompletedEventHandler(secondary_fetch_Failed);
            tertiary_fetch.Completed += new ApiCompletedEventHandler(tertiary_fetch_Completed);
            tertiary_fetch.Failed += new ApiCompletedEventHandler(tertiary_fetch_Failed);
 
        }

        void tertiary_fetch_Failed(object sender, APICompletedEventArgs e)
        {
            RaiseFailed(e);
        }

        void tertiary_fetch_Completed(object sender, APICompletedEventArgs e)
        {
            RaiseCompleted(e);
        }

        public void StartFetch(string Yt_Source)
        {
            Disposed = false;
            this.youtube_source = Yt_Source;
            primary_fetch.GetStreamURL(this.youtube_source);
        }

        void secondary_fetch_Failed(object sender, APICompletedEventArgs e)
        {
            if(!Disposed)
                this.tertiary_fetch.GetStreamURL(this.youtube_source);
        }

        void secondary_fetch_Completed(object sender, APICompletedEventArgs e)
        {
            RaiseCompleted(e);
        }

        void primary_fetch_Failed(object sender, APICompletedEventArgs e)
        {
            if (!Disposed)
                this.secondary_fetch.GetStreamURL(this.youtube_source);
        }

        void primary_fetch_Completed(object sender, APICompletedEventArgs e)
        {
            RaiseCompleted(e);
        }

        public void Dispose()
        {
            primary_fetch.Dispose();
            secondary_fetch.Dispose();
            tertiary_fetch.Dispose();
            Disposed = true;
        }
    }
}
