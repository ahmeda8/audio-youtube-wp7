using System;
using Microsoft.Phone.BackgroundTransfer;

namespace Resources
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
}
