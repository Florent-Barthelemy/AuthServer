using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer
{
    static class Utils
    {
        public static void CopyArray<T>(T[] src, int srcOffset, ref T[] dest, int destOffset, int count)
        {
            if (count == 0)
                return;

            int cnt = 0;
            do
            {
                dest[destOffset + cnt] = src[srcOffset + cnt];
                cnt++;
            }
            while (cnt < count);
        }

        /// <summary>
        /// METHOD FROM INTERNET
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public static string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }
    }
}
