using System;
using System.Net;
using Microsoft.Phone.Info;

namespace ResourceLibrary
{
    public class ErrorLogging  
    {
        public static void Log(string class_name,string message,string tag,string hash)
        {
            string manufacturer = DeviceStatus.DeviceManufacturer;
            string devicename = DeviceStatus.DeviceName;
            string firmware = DeviceStatus.DeviceFirmwareVersion;
            string device_hardware = DeviceStatus.DeviceHardwareVersion;
            long device_ram = DeviceStatus.DeviceTotalMemory;
            long app_mem = DeviceStatus.ApplicationCurrentMemoryUsage;
            long app_max_mem = DeviceStatus.ApplicationPeakMemoryUsage;
            object device_id_hash;
            DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out device_id_hash);
            byte[] device_id = new byte[20];
            device_id = (byte[])device_id_hash;
            string device_id_str = Convert.ToBase64String(device_id);
#if DEBUG
            System.Diagnostics.Debug.WriteLine(manufacturer + " msg: " + message);
#else

            WebMethodErrorLogging wm = new WebMethodErrorLogging();
            string url = "http://eminence.webatu.com/errorlogs/diagnostics.php";
            string post = "application=musicyoutube" +
                "&deviceid=" + HttpUtility.UrlEncode(device_id_str) +
                "&devicemanu=" + manufacturer +
                "&devicemodel=" + devicename +
                "&errorclass=" + class_name +
                "&message=" + message +
                "&classhash=" +hash +
                "&tags=" + tag;
            //post = HttpUtility.UrlEncode(post);
            wm.POST_LOG(url,post);
#endif

 
        }
    }

    public class WebMethodErrorLogging : WebMethod
    {
        public void POST_LOG(string url,string message)
        {
            base.POST(url, message);
        }

        public override void GET_Method_CallBack(IAsyncResult res)
        {
            throw new NotImplementedException();
        }

        public override void POST_Method_CallBack(IAsyncResult res)
        {
           
        }
    }
}
