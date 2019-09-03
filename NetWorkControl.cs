using Microsoft.Win32;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;

namespace WpfApp
{
    class NetWorkControl
    {
        public static List<string> NetWorkList()
        {
            string manage = "SELECT * From Win32_NetworkAdapter";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(manage);
            ManagementObjectCollection collection = searcher.Get();
            List<string> netWorkList = new List<string>();

            foreach (ManagementObject obj in collection)
            {
                PropertyDataCollection collect = obj.Properties;
                string DeviceID = obj["DeviceID"].ToString();
                string id = obj["PNPDeviceID"].ToString();
                if (obj["AdapterType"] != null)
                {
                    string adapterType = obj["AdapterType"].ToString();
                }
                if (obj["AdapterTypeID"] != null) {
                    string AdapterTypeID = obj["AdapterTypeID"].ToString();
                }
                if (obj["SystemName"] != null) {
                    string SystemName = obj["SystemName"].ToString();
                }
                if (obj["GUID"] != null)
                {
                    string GUID = obj["GUID"].ToString();
                    obj.InvokeMethod("Disable", null);
                    //obj.InvokeMethod("Enable", null);


                    object NetEnabled = obj["NetEnabled"];

                    //obj.SetPropertyValue("NetEnabled",false);
                    //object NetEnabled2 = obj["NetEnabled"];

                    //obj["NetEnabled"] = false;

                }

                netWorkList.Add(obj["Name"].ToString());

            }
            return netWorkList;
        }

        public static void ShowNetworkInterfaceMessage()
        {
            NetworkInterface[] fNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in fNetworkInterfaces)
            {
                //无线网卡
                if (adapter.NetworkInterfaceType.Equals(NetworkInterfaceType.Wireless80211)) {
                    
                }
                string fRegistryKey = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + adapter.Id + "\\Connection";
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(fRegistryKey, false);
                if (rk != null) {
                    //删除注册表
                    // 区分 PnpInstanceID
                    // 如果前面有 PCI 就是本机的真实网卡
                    // MediaSubType 为 01 则是常见网卡，02为无线网卡。
                    string fPnpInstanceID = rk.GetValue("PnpInstanceID", "").ToString();
                    //string mediaSubTypevalue = rk.GetValue("MediaSubType").ToString();

                }
            }
        }
    }
}
