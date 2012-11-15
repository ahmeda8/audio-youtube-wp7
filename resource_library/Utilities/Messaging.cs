using System;
using System.Text;

namespace ResourceLibrary
{
    public class Messaging : IMessaging
    {
        private string message;
        private static Messaging StaticMessagingInstance = null;

        //factory

        public static Messaging GetInstance()
        {
            if(StaticMessagingInstance == null)
                StaticMessagingInstance = new Messaging();
            return StaticMessagingInstance;
        }

        public void SetMessage(string Message)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(Message);
#endif
            this.message = Message;
            RaiseChanged();
        }

        public string GetMessage()
        {
            return this.message;
        }

        public event ApiCompletedEventHandler Changed;

        public virtual void RaiseChanged()
        {
            if (Changed != null)
                Changed(this, new APICompletedEventArgs(message));

        }
    }
}
