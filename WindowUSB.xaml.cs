using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace WpfApp
{
    /// WindowUSB.xaml 的交互逻辑
    /// </summary>
    public partial class WindowUSB : Window
    {

        FileHandle fileHandle = new FileHandle();
        FindUDisk findUDisk = new FindUDisk();
        public const int WM_DEVICECHANGE = 0x219;//U盘插入后，OS的底层会自动检测到，然后向应用程序发送“硬件设备状态改变“的消息
        public const int DBT_DEVICEARRIVAL = 0x8000;  //就是用来表示U盘可用的。一个设备或媒体已被插入一块，现在可用。
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;  //一个设备或媒体片已被删除。

        string currentItemText = null;
        int currentItemIndex = -1;

        private List<string> rightDataList = new List<string>();

        private ArrayList leftDataList = new ArrayList();

        public WindowUSB()
        {
            InitializeComponent();
        }

        //是否属于closing事件关闭,默认是事件关闭
        private bool isEventClosing = true;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogUtils.writeLog("开始初始化winddowUsb");
            //matchDriveLetterWithSerial();
            // Get data from somewhere and fill in my local ArrayList
            initLeftDataList();
            rightDataList = fileHandle.readDataFile(false);
            LogUtils.writeLog("WindowUSB:45:初始化右边的数据集！");
            foreach (String s in rightDataList) {
                LogUtils.writeLog("WindowUSB:47:右边数据集："+s);
            }
            // Bind ArrayList with the ListBox
            //LeftListBox.ItemsSource = leftDataList;
            RightListBox.ItemsSource = rightDataList;
        //DeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;

        }

        private void initLeftDataList() {
            LogUtils.writeLog("WindowUSB:57:初始化左边的数据集！");
            leftDataList = findUDisk.matchDriveLetterWithSerial();
            LeftListBox.ItemsSource = leftDataList;
        }
       
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (LeftListBox.SelectedValue != null)
            {
                currentItemText = LeftListBox.SelectedValue.ToString();
                currentItemIndex = LeftListBox.SelectedIndex;
                rightDataList.Add(currentItemText);
                //RightListBox.Items.Add(currentItemText);
                if (leftDataList != null)
                {
                    leftDataList.RemoveAt(currentItemIndex);
                }
                // Refresh data binding
                ApplyDataBinding();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

            if (RightListBox.SelectedValue != null)
            {
                // Find the right item and it's value and index
                currentItemText = RightListBox.SelectedValue.ToString();
                currentItemIndex = RightListBox.SelectedIndex;
                // Add RightListBox item to the ArrayList
                leftDataList.Add(currentItemText);
                if (rightDataList != null)
                {
                    rightDataList.RemoveAt(currentItemIndex);
                }
                //RightListBox.Items.RemoveAt(RightListBox.Items.IndexOf(RightListBox.SelectedItem));

                // Refresh data binding
                ApplyDataBinding();
            }
        }

        private void ApplyDataBinding()
        {
            LeftListBox.ItemsSource = null;
            // Bind ArrayList with the ListBox
            LeftListBox.ItemsSource = leftDataList;

            RightListBox.ItemsSource = null;
            RightListBox.ItemsSource = rightDataList;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool flag = false;
            if (rightDataList.Count != 0)
            {
                StringBuilder str = new StringBuilder();
                foreach (string s in rightDataList)
                {
                    str.Append(s + "\r\n");
                }
                flag = fileHandle.writeDataFile(str.ToString());
            }
            else {
                flag = fileHandle.writeDataFile("");
            }

            if (flag) {
                try {
                    MessageBox.Show("保存成功");
                    MonitorUSB monitorUSB = new MonitorUSB();
                    monitorUSB.Show();
                    this.isEventClosing = false;
                    this.Close();
                }
                catch (Exception ex) {
                    LogUtils.writeLog(ex.ToString());
                }

            }
        }

        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null ) {
                source.AddHook(WdProc);
            }
        }

        private IntPtr WdProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
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
                                    Dispatcher.BeginInvoke(new Action(() => initLeftDataList()), DispatcherPriority.Background);
                                    return true;
                                }
                                return false;
                            });
                            break;
                        case DBT_DEVICEREMOVECOMPLETE:
                            Dispatcher.BeginInvoke(new Action(() => {
                                LeftListBox.ItemsSource = null;
                            }), DispatcherPriority.Background);
                            MessageBox.Show("U盘卸载");
                            break;
                        default:
                            break;
                    }
                }
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
            return IntPtr.Zero;
        }

        private void MonitorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MonitorUSB monitorUSB = new MonitorUSB();
                monitorUSB.Show();
                this.isEventClosing = false;
                this.Close();
            }
            catch (Exception ex){
                LogUtils.writeLog(ex.ToString());
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isEventClosing) {
                MonitorUSB monitorUSB = new MonitorUSB();
                monitorUSB.Show();
            }
         
        }
    }
}
