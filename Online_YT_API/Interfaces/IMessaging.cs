using System;
using System.Text;

namespace Resources
{
    interface IMessaging
    {
        event ApiCompletedEventHandler Changed;
        void SetMessage(string Message);
        string GetMessage();
    }
}
