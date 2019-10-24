using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace WpfApp
{
    class FileHandle
    {
        //文件读取
        public List<string> readDataFile(bool isShowMsg)
        {
            string currentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            List<string> list = new List<string>();
            String path = currentDirectory + "U.DAT";// 要读取的文件
            //判断是否含有指定文件
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                //fs.Seek(44, SeekOrigin.Begin);// 定位到第44个字节
                //定义存放文件信息的字节数组
                byte[] bytes = new byte[fs.Length];
                //读取文件信息
                fs.Read(bytes, 0, bytes.Length);
                //将得到的字节型数组重写编码为字符型数组
                string s = Encoding.ASCII.GetString(bytes);
                if (!string.IsNullOrEmpty(s))
                {
                    s = s.Trim();
                    list = s.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
                }
                Console.WriteLine(s);
                //关闭流
                fs.Close();
            }
            else
            {
                if (isShowMsg) {
                    MessageBox.Show("您查看的文件不存在！");
                }
            }
            return list;
        }


        //写入文件
        public bool writeDataFile(string str)
        {
            bool flag = false;
            string currentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            String saveFile = currentDirectory + "U.DAT";//要保存的文件
            FileStream writeStream = File.Create(saveFile);
            //FileStream writeStream = File.OpenWrite(saveFile);// 以写的方式打开

            //byte[] array = System.Text.Encoding.ASCII.GetBytes(str2);  //数组array为对应的ASCII数组    
            //string ASCIIstr2 = null;
            /*for (int i = 0; i < array.Length; i++)
            {
                int asciicode = (int)(array[i]);
                ASCIIstr2 += Convert.ToString(asciicode);//字符串ASCIIstr2 为对应的ASCII字符串
            }*/

            byte[] bytes = Encoding.ASCII.GetBytes(str);
            //writeStream.Seek(44, SeekOrigin.Begin);// 定位，在第44个字节处写入
            writeStream.Write(bytes, 0, bytes.Length);// 将准备好的数组写入文件。newData是包含要写入文件的byte类型数组；0是数组中的从零开始的字节偏移量，从此处开始将字节复制到该流；newData.Length是要写入的字节数。这句话的意思是从44个字节开始把数组内容从头到尾写进去，修改下参数如writeStream.Write(newData, 1, newData.Length-1)是把数组从第二个到倒数第一个写进去
            writeStream.Close();// 关闭文件
            flag = true;
            return flag;
        }


    }
}
