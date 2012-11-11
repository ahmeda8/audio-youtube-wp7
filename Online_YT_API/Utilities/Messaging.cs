using System;
using System.Text;

namespace Resources
{
    public class Messaging : IMessaging
    {
        private string message;

        public void SetMessage(string Message)
        {
            this.message = Message;
            RaiseChanged();
        }

        public string GetMessage()
        {
            return this.message;
        }

        public event ApiCompletedEventHandler Changed;

        private virtual void RaiseChanged()
        {
            if (Changed != null)
                Changed(this, new APICompletedEventArgs(message));

        }
    }
}
