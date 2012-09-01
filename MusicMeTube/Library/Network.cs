namespace MusicMeTube
{
    public class Network
    {
        public static bool IsConnected()
        {
#if DEBUG
            return true;
#else
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
#endif

        }
    }
}
