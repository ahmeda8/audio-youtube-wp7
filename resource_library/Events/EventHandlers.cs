using System;
using Microsoft.Phone.BackgroundTransfer;

namespace ResourceLibrary
{
    public class APICompletedEventArgs : EventArgs
    {
        public String Response { get; set; }
        public APICompletedEventArgs(String Response)
        {
            this.Response = Response;
        }
    }

    public delegate void ApiCompletedEventHandler(object sender, APICompletedEventArgs e);


    public class FileDownloadEvntArgs : EventArgs
    {
        public String Message { get; set; }
        public FileDownloadEvntArgs(string msg)
        {
            Message = msg;
        }

    }
    public delegate void FileDownloadEvntHandler(object sender,FileDownloadEvntArgs e);

    public class GenericEvntArgs : EventArgs
    {
        public object GenericObject;
        public GenericEvntArgs(object obj)
        {
            this.GenericObject = obj;
        }
    }

    public delegate void GenericEvntHandler(object sender);
}
