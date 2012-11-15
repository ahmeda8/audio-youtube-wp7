using System;
using System.ComponentModel;

namespace ResourceLibrary
{
    public class Model : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify Silverlight that a property has changed.
        public void NotifyPropertyChanged(string propertyName,Object o)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(o, new PropertyChangedEventArgs(propertyName));
            }
        }
        
    }
}
