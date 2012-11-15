using System;
using System.Text;

namespace ResourceLibrary
{
    interface IMessaging
    {
        event ApiCompletedEventHandler Changed;
        void SetMessage(string Message);
        string GetMessage();
    }
}
