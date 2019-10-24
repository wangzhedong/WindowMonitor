using System;
using System.IO;
using System.Windows;

namespace WpfApp
{
    class LogUtils
    {

        public static void writeLog(String str) {
            string currentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string dirPath = currentDirectory + "logs\\";           
            string logFileName = "log.txt";
            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
            StreamWriter sw = new StreamWriter(dirPath + logFileName, true);
            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": "+str+":");
            sw.Close();
        }

    }
}
