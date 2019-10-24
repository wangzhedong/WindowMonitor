using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace WpfApp
{
    /// <summary>
    /// 监听usb接口，移除不在列表中的u盘
    /// </summary>
    public partial class MonitorUSB : Window
    {

        public const int WM_DEVICECHANGE = 0x219;//U盘插入后，OS的底层会自动检测到，然后向应用程序发送“硬件设备状态改变“的消息
        public const int DBT_DEVICEARRIVAL = 0x8000;  //就是用来表示U盘可用的。一个设备或媒体已被插入一块，现在可用。
        public const int DBT_CONFIGCHANGECANCELED = 0x0019;  //要求更改当前的配置（或取消停靠码头）已被取消。
        public const int DBT_CONFIGCHANGED = 0x0018;  //当前的配置发生了变化，由于码头或取消固定。
        public const int DBT_CUSTOMEVENT = 0x8006; //自定义的事件发生。 的Windows NT 4.0和Windows 95：此值不支持。
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;  //审批要求删除一个设备或媒体作品。任何应用程序也不能否认这一要求，并取消删除。
        public const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;  //请求删除一个设备或媒体片已被取消。
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;  //一个设备或媒体片已被删除。
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;  //一个设备或媒体一块即将被删除。不能否认的。
        public const int DBT_DEVICETYPESPECIFIC = 0x8005;  //一个设备特定事件发生。
        public const int DBT_DEVNODES_CHANGED = 0x0007;  //一种设备已被添加到或从系统中删除。
        public const int DBT_QUERYCHANGECONFIG = 0x0017;  //许可是要求改变目前的配置（码头或取消固定）。
        public const int DBT_USERDEFINED = 0xFFFF;  //此消息的含义是用户定义的
        public const uint GENERIC_READ = 0x80000000;
        public const int GENERIC_WRITE = 0x40000000;
        public const int FILE_SHARE_READ = 0x1;
        public const int FILE_SHARE_WRITE = 0x2;
        public const int IOCTL_STORAGE_EJECT_MEDIA = 0x2d4808;

        //是否真的关闭窗口,默认假关闭，隐藏
        public bool isReadlClose = false;
        HotKey hotKey = null;
        public MonitorUSB()
        {
            InitializeComponent();
        }

        private void HotKey_OnHotKey()
        {
            if (this.IsVisible == true)
            {
                this.Hide();
            }
            else {
                this.Show();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PasswordWindow passwordWindow = new PasswordWindow();
            passwordWindow.Show();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null)
            {
                source.AddHook(WdProc);
            }
        }

        private IntPtr WdProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                if (msg == WM_DEVICECHANGE)
                {
                    switch (wParam.ToInt32())
                    {
                        case DBT_DEVICEARRIVAL:
                            DriveInfo[] s = DriveInfo.GetDrives();
                            s.Any(t =>
                            {
                                if (t.DriveType == DriveType.Removable)
                                {
                                    //MessageBox.Show("U盘插入,盘符为：" + t.Name);
                                    LogUtils.writeLog("MonitorUSB:87:U盘插入,盘符为：" + t.Name);
                                    DispatcherOperation d = Dispatcher.BeginInvoke(new Action(() => removeU()), DispatcherPriority.Background);
                                    return true;
                                }
                                return false;
                            });
                            break;
                        case DBT_DEVICEREMOVECOMPLETE:
                            LogUtils.writeLog("MonitorUSB:95:U盘卸载");
                            //MessageBox.Show("");
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return IntPtr.Zero;
        }


        private void removeU()
        {
            List<DriverBase> driverBaseList = FindUDriver.GetDrivers();
            if (driverBaseList != null && driverBaseList.Count > 0) {
                FileHandle handle = new FileHandle();
                List<string> saveNum = handle.readDataFile(false);
                if (saveNum.Count > 0)
                {
                    foreach (DriverBase b in driverBaseList)
                    {
                        bool boo = saveNum.Contains(b.SerialNum);
                        if (!boo) {
                            LogUtils.writeLog("MonitorUSB:123:准备卸载u盘：" + b.DriverName);
                            RemoveUsbDevice(b.DriverName);
                        }
                       /* foreach (string num in saveNum) {
                            if (!b.SerialNum.Equals(num)) {
                            }
                        }*/
                    }
                }
                else {
                    foreach (DriverBase b in driverBaseList) {
                        LogUtils.writeLog("MonitorUSB:134:准备卸载u盘：" + b.DriverName);
                        RemoveUsbDevice(b.DriverName);
                    }
                }

            }
        }



        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesireAccess,
        uint dwShareMode,
        IntPtr SecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );

        /// <summary>
        /// 卸载当前指定的U盘存储设备
        /// </summary>
        /// <returns>卸载结果</returns>
        private bool RemoveUsbDevice(string strCurUsb)
        {
            //第一个参数filename与普通文件名有所不同，设备驱动的“文件名”(常称为“设备路径”)形式固定为“\\.\DeviceName”, 如 string filename = @"\\.\I:";
            string filename = @"\\.\" + strCurUsb.Remove(2);
            //打开设备，得到设备的句柄handle.
            IntPtr handle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);

            // 向目标设备发送设备控制码，也就是发送命令。IOCTL_STORAGE_EJECT_MEDIA  弹出U盘。
            uint byteReturned;
            bool result = DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);

            //MessageBox.Show(result ? "U盘已退出" : "U盘退出失败");
            return result;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.isReadlClose)
            {
                bool f = hotKey.UnRegHotKey();
                hotKey.OnHotKey -= HotKey_OnHotKey;
                hotKey = null;
                LogUtils.writeLog("热键注销成功："+f.ToString());
            }else {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogUtils.writeLog("开始注册热键");
            hotKey = new HotKey(this, HotKey.KeyFlags.MOD_ALT, System.Windows.Forms.Keys.F7);
            hotKey.OnHotKey += HotKey_OnHotKey;
            if (App.isHideWindow) {
                this.Hide();
            }
            removeU();
            //NetWorkControl.NetWorkList();
            //NetWorkControl.ShowNetworkInterfaceMessage();
        }
    }
}
