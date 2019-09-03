using System;

namespace WpfApp
{
    class DriverBase
    {
        public DriverBase()
        {

        }
        public string DeviceID { get; set; }

        public string DriverName { get; set; }

        public string VolumeSerialNum { get; set; }

        public double TotalSpace { get; set; }

        public double FreeSpace { get; set; }

        public string SerialNum { get; set; }

        public bool IsOverload { get; set; }
    }
}