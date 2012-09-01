using System;

namespace Resources
{
    interface IOnlineServiceProvidersApi
    {
        event ApiCompletedEventHandler Completed;
        event ApiCompletedEventHandler Failed;
        void GetStreamURL(String Youtube_URL);
        string Youtube_Source_URL { get; set; }
        string Host_URL { get; set; }
    }
}
