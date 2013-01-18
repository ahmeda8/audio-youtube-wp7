using System;
using System.Windows.Media;

namespace ResourceLibrary
{
    public class Entry : Model
    {
        private string _id;
        public string Id
        {
            get { return _id; }
            set { _id = value; NotifyPropertyChanged("Id",this); }
        }

        private string _playlistid;
        public string PlaylistID
        {
            get { return _playlistid; }
            set { _playlistid = value; NotifyPropertyChanged("PlaylistID", this); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged("Title",this); }
        }

        private string _source;
        public string Source
        {
            get { return _source; }
            set { _source = value; NotifyPropertyChanged("Source", this); }
        }

        private string _offline;
        public string Offline
        {
            get { return _offline; }
            set { _offline = value; NotifyPropertyChanged("Offline", this); }
        }

        private SolidColorBrush _availibilityColor;
        public SolidColorBrush AvailablityColor
        {
            get { return _availibilityColor; }
            set { _availibilityColor = value; NotifyPropertyChanged("AvailablityColor", this); }
        }

        private string _imgSrc;
        public string ImageSource
        {
            get { return _imgSrc; }
            set { _imgSrc = value; NotifyPropertyChanged("ImageSource", this); }
        }

        private string _imgSrclow;
        public string ImageSourceLow
        {
            get { return _imgSrclow; }
            set { _imgSrclow = value; NotifyPropertyChanged("ImageSourceLow", this); }
        }

        private int _count;
        public int Count
        {
            get { return _count; }
            set { _count = value; NotifyPropertyChanged("Count", this); }
        }

        private DateTime _updated;
        public DateTime Updated
        {
            get { return _updated; }
            set { _updated = value; NotifyPropertyChanged("Updated", this); }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; NotifyPropertyChanged("Duration", this); }
        }

        private string _credit;
        public string Credit
        {
            get { return _credit; }
            set { _credit = value; NotifyPropertyChanged("Credit", this); }
        }

        private string _entryid;
        public string EntryID
        {
            get { return _entryid; }
            set { _entryid = value; NotifyPropertyChanged("EntryID", this); }
        }
    }
}
