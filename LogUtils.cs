using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    class LogUtils
    {

        public static void writeLog(String str) {
            string currentDirectory = Environment.CurrentDirectory;
            string logFileName = "log.txt";
            string dirPath = currentDirectory + "\\logs\\";
            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
            StreamWriter sw = new StreamWriter(dirPath + logFileName, true);
            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": "+str);
            sw.Close();
        }

    }
}
