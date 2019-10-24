using System.Collections.Generic;
using System.Management;

namespace WpfApp
{
    class FindUDriver
    {
        public static List<DriverBase> GetDrivers()
        {
            List<DriverBase> drivers = new List<DriverBase>();
            string strQuery = "select * from Win32_DiskDrive Where InterfaceType = 'USB'";
            SelectQuery sq = new SelectQuery(strQuery);
            ManagementObjectSearcher mos = new ManagementObjectSearcher(sq);
            foreach (ManagementObject disk in mos.Get())
            {
                string DeviceID = disk["DeviceID"].ToString();
                foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + DeviceID + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
                {
                    string query = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass = Win32_LogicalDiskToPartition";
                    foreach (ManagementObject disk1 in new ManagementObjectSearcher(query).Get())
                    {
                        DriverBase b = new DriverBase();
                        string diviceID = disk1["DeviceID"].ToString().Trim();
                        /*if (!IsNTFS(diviceID))
                        {
                            continue;
                        }*/
                        string Name = disk1["Name"].ToString().Trim();
                        /*string freeSpace = disk1["FreeSpace"].ToString().Trim();
                         string size = disk1["Size"].ToString().Trim();
                         if (!string.IsNullOrEmpty(partitionName) && Name.ToUpper() != partitionName.ToUpper())
                         {
                             continue;
                         }
                        b.DriverName = Helper.DriverHelper.GetVolumnLabel(diviceID);
                        b.TotalSpace = double.Parse(size);
                        b.FreeSpace = double.Parse(freeSpace);*/
                        b.DriverName = Name + "\\";
                        b.DeviceID = diviceID;
                        //b.IsOverload = (b.TotalSpace - b.FreeSpace) / b.TotalSpace > 0.7;
                        //这个SerialNum可能会不准确
                        //b.SerialNum = disk["SerialNumber"] == null ? "" : disk["SerialNumber"].ToString().Trim();

                        string pnpdeviceid = disk["PNPDeviceID"] == null ? "" : disk["PNPDeviceID"].ToString().Trim();
                        if (!string.IsNullOrEmpty(pnpdeviceid)) {
                            b.SerialNum = parseSerialFromDeviceID(pnpdeviceid);
                        }
                        //b.VolumeSerialNum = GetVolumeSerialNumber(diviceID);
                        if (!string.IsNullOrEmpty(b.SerialNum))
                            drivers.Add(b);

                    }
                }
            }
            return drivers;
        }


        /// <summary>
        /// Get driver by partition name
        /// </summary>
        /// <param name="name">name should be "C:" or "D"</param>
        /// <returns></returns>
        public static DriverBase GetDriverByName(string name)
        {
            string strQuery = "select * from Win32_LogicalDisk where DeviceID='" + name + "'";

            SelectQuery sq = new SelectQuery(strQuery);
            ManagementObjectSearcher mos = new ManagementObjectSearcher(sq);
            DriverBase driver = null;
            foreach (ManagementObject disk in mos.Get())
            {
                driver = new DriverBase();
                driver.DeviceID = disk["DeviceID"].ToString();
                driver.DriverName = disk["VolumeName"].ToString();
                driver.VolumeSerialNum = disk["VolumeSerialNumber"].ToString();
                driver.TotalSpace = double.Parse(disk["Size"].ToString());
                if (disk["FileSystem"].ToString() != "NTFS")
                {
                    return null;
                }
                driver.FreeSpace = double.Parse(disk["FreeSpace"].ToString());
                driver.IsOverload = (driver.TotalSpace - driver.FreeSpace) / driver.TotalSpace > 0.7;
                //driver.SerialNum = GetSeiralByDevice(driver.DeviceID);
                if (!string.IsNullOrEmpty(driver.SerialNum))
                    return driver;
            }
            return driver;
            //return DriverBase.GetDrivers(InterfaceType.ALL, name).FirstOrDefault();
        }

        // fix cannot get serial number of some flash driver of WMI
        // http://stackoverflow.com/questions/1176053/read-usb-device-serial-number-in-c-sharp/1176089#1176089
        private static string parseSerialFromDeviceID(string deviceId)
        {
            string[] splitDeviceId = deviceId.Split('\\');
            string[] serialArray;
            string serial;
            int arrayLen = splitDeviceId.Length - 1;

            serialArray = splitDeviceId[arrayLen].Split('&');
            serial = serialArray[0];

            return serial;
        }


    }
}
