using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QRCoder;

using System.Drawing;
using System.IO;

namespace gd4lib
{
    public class utils
    {
        public System.Drawing.Image genQr(string txt)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            var QRCodeData = qrGenerator.CreateQrCode(txt, QRCodeGenerator.ECCLevel.M);
            QRCode QRCode = new QRCode(QRCodeData);
            Bitmap qrCodeImage = QRCode.GetGraphic(20);
            return qrCodeImage;
        }

        //public void log2(string message)
        //{
        //    string tm = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss").ToString();
        //    File.AppendAllText( "\\log.txt", tm+":"+message);
        //}

        //public  void log(string strLog)
        //{
        //    string tm = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss").ToString();
        //    string logFilePath = @"C:\Logs\Log-" + System.DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";
        //    FileInfo logFileInfo = new FileInfo(logFilePath);
        //    DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
        //    if (!logDirInfo.Exists) logDirInfo.Create();
        //    using (FileStream fileStream = new FileStream(logFilePath, FileMode.Append))
        //    {
        //        using (StreamWriter log = new StreamWriter(fileStream))
        //        {
        //            log.WriteLine(tm+" : "+strLog);
        //        }
        //    }
        //}
    }
}
