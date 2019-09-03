using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PasswordWindow : Window
    {

        public PasswordWindow()
        {
            InitializeComponent();
        }

        
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox box =(PasswordBox) sender;
                string password = box.Password;
                // 使用一个IntPtr类型值来存储加密字符串的起始点  
                //IntPtr p = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.PasswordBox_KeyDown.SecurePassword);

                // 使用.NET内部算法把IntPtr指向处的字符集合转换成字符串  
                //string password = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(p);

                // 顺便校验一下  
                if (string.IsNullOrEmpty(password) || !password.Equals("123456"))
                {
                    MessageBox.Show("密码错误", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }
                else {
                    //MessageBox.Show("密码正确", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is MonitorUSB) {
                            MonitorUSB uu =(MonitorUSB) window;
                            uu.isReadlClose = true;
                            window.Close();
                        }
                        //MessageBox.Show(window.Title);
                    }
                    WindowUSB usb = new WindowUSB();
                    usb.Show();
                    this.Close();
                }
            }
        }

      
    }
}
