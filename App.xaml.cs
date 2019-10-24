using System;
using System.Diagnostics;
using System.Windows;

namespace WpfApp
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //是否隐藏窗口，默认不隐藏
        public static bool isHideWindow = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            Process thisProc = Process.GetCurrentProcess();
            string exeName = thisProc.ProcessName;
            // Get Reference to the current Process
            Process[] process = Process.GetProcessesByName(exeName);
            // Check how many total processes have the same name as the current one
            if (process.Length > 1)
            {
                // If ther is more than one, than it is already running.
                MessageBox.Show("应用程序已经在后台启动");
                Application.Current.Shutdown();
                return;
            }
            bool IsStart = true;
            string currentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string exePath = currentDirectory+exeName+".exe autoStart";
            bool exist = AutoStart.IsExistKey(exeName, exePath);
            //开机自启动的时候是带参数启动的，当第一个参数是autoStart时，自动隐藏窗口
            if (e.Args.Length > 0 && (e.Args[0].Equals("autoStart")))
            {
                LogUtils.writeLog("App:36:开机自启动并隐藏窗口");
                isHideWindow = true;
            }
            if (!exist && IsStart)
            {
                LogUtils.writeLog("App:41:注册开机自启动注册表"+ exePath);
                AutoStart.SelfRunning(IsStart, exeName, exePath);
            }
            else if (exist && !IsStart)
            {
                LogUtils.writeLog("App:45:注册开机自启动注册表" + exePath);
                AutoStart.SelfRunning(!IsStart, exeName, exePath);
            }
 
            base.OnStartup(e);

        }
    }
}
