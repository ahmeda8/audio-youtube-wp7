using System;
using System.Text;

namespace Resources
{
    interface IDownloader
    {
        event GenericEvntHandler Completed;
        void Start();
        void Abort();
        Entry Current {get;set;}
    }
}
