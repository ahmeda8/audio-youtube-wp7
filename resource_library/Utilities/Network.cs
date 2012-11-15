using Microsoft.Phone.Net.NetworkInformation;

namespace ResourceLibrary
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

        public static bool IsOnCellularNetwork()
        {
#if DEBUG
            return true;
#else
            if (NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                return false;
            else
                return true;
#endif
        }
    }
}
