using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDSender.cls;

namespace BDSender
{
    class clsconvertdate
    {

        public string convertdate_YYYY_MM_DD_HH_mm_ss_EN_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            if (val_ != "")
            {
                DT += val_.Substring(0, 4) + "-";
                DT += val_.Substring(5, 2) + "-";
                DT += val_.Substring(8, 2);
                DT += DateTime.Now.ToString(" HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }
            return DT;
        }
        //public Image genQr(string txt)
        //{
        //    QRCodeGenerator qrGenerator = new QRCodeGenerator();
        //    var QRCodeData = qrGenerator.CreateQrCode(txt, QRCodeGenerator.ECCLevel.M);
        //    QRCode QRCode = new QRCode(QRCodeData);
        //    Bitmap qrCodeImage = QRCode.GetGraphic(20);
        //    return qrCodeImage;
        //}
        public string convertdate_HH_mm_ss_EN_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            if (val_ != "")
            {
                DT += DateTime.Now.ToString(" HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }
            return DT;
        }
        public string convertdate_YYYY_MM_DD_HH_00_EN_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            if (val_ != "")
            {
                DT += val_.Substring(0, 4) + "-";
                DT += val_.Substring(5, 2) + "-";
                DT += val_.Substring(8, 2);
                DT += " 00:00:00";
            }
            return DT;
        }
        public string convertdate_YYYY_MM_DD_HH_EN_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            if (val_ != "")
            {
                DT += val_.Substring(0, 4) + "-";
                DT += val_.Substring(5, 2) + "-";
                DT += val_.Substring(8, 2);
            }
            return DT;
        }
        public string convert_en(string val)
        {
            DateTime dt = new DateTime();
            try
            {
                if (val != "")
                {
                    dt = DateTime.Parse(val);

                }
                return dt.ToString("yyyy-MM-dd", CultureInfo.GetCultureInfo("en-US"));
            }
            catch (Exception e)
            {
                return dt.ToLongDateString();
            }


        }
        public string convert_en_time(string val)
        {
            DateTime dt = new DateTime();
            try
            {
                if (val != "")
                {
                    dt = DateTime.Parse(val);

                }
                return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.GetCultureInfo("en-US"));
            }
            catch (Exception e)
            {
                return dt.ToLongDateString();
            }


        }
        public string convert_en_y(string val)
        {
            DateTime dt = new DateTime();
            try
            {
                if (val != "")
                {
                    dt = DateTime.Parse(val);

                }
                return dt.ToString("yyyy", CultureInfo.GetCultureInfo("en-US"));
            }
            catch (Exception e)
            {
                return dt.ToLongDateString();
            }


        }
        public string convertdate_YYYY_EN(string val)
        {
            int buddhistYear = 0;
            var dt = Convert.ToDateTime(val).ToString("yyyy-MM-dd");
            string val_ = dt.ToString();
            string DT = "";
            if (val_ != "")
            {
                DT += val_.Substring(0, 4);
                if (Convert.ToInt32(DT) > 2500)
                {
                    buddhistYear = Convert.ToInt32(DT) - 543;
                    DT = buddhistYear.ToString();

                }
            }
            return DT;
        }
        public string convertdate_YYYY_MM_DD_TH_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            if (val_ != "")
            {
                DT += val_.Substring(0, 4);
                if (Convert.ToInt32(DT) > 2000 && Convert.ToInt32(DT) < 2500)
                {
                    buddhistYear = Convert.ToInt32(DT) + 543;
                    DT = buddhistYear.ToString();

                }
                DT += "-";
                DT += val_.Substring(5, 2) + "-";
                DT += val_.Substring(8, 2);
                //DT += DateTime.Now.ToString(" HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }
            return DT;
        }
        public string convertdate_YYYYMMDD_TH_NEW(string val)
        {
            string val_ = val.ToString();
            string DT = "";
            int buddhistYear = 0;
            string[] exp;
            string date;
            string[] year;
            if (val_ != "")
            {
                DT += val_.Substring(0, 4);
                if (Convert.ToInt32(DT) > 2000 && Convert.ToInt32(DT) < 2500)
                {
                    buddhistYear = Convert.ToInt32(DT) + 543;
                    DT = buddhistYear.ToString();

                }
                DT += val_.Substring(5, 2);
                DT += val_.Substring(8, 2);
                DT += DateTime.Now.ToString("HHmmss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            }

            return DT;
        }
    }
}
