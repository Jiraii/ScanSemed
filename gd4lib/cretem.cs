using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace gd4lib
{
    public class cretem
    {
        private string exportPath;
        public string OrderHeader;
        public string dateNow;
        //public DateTime dateTime;
        public int runningNum;
        public string OrderNum;
        public string xmlMessage;

        public cretem(string xmlExprtPath)
        {
            //DateTime dateTime = DateTime.UtcNow.Date;
            this.exportPath = xmlExprtPath;
            this.dateNow = getDateNow();
            this.OrderHeader = RandomString(5);
            this.runningNum = 0;
        }

        public string data2apmed(List<IPD> data)
        {
            this.xmlMessage = "";

            if (data[0].OrderNum == "")
            {
                if (this.dateNow != getDateNow())
                {
                    this.dateNow = getDateNow();
                    this.runningNum = 1;
                }
                else
                {
                    this.runningNum += 1;
                }

                this.OrderNum = this.OrderHeader + String.Format("{0:D5}", this.runningNum);
            }else
            {
                this.OrderNum = data[0].OrderNum;
            }

            OrderInfo obj = new OrderInfo();
            obj.HsptCd = "1";          
            obj.DptmtCd = data[0].DptmtCd.Trim();
            obj.WardCd = isNullZero(data[0].WardCd).Trim();
            obj.DataClsf = data[0].DataClsf.Trim();
            obj.InOutClsf = data[0].InOutClsf.Trim();
            obj.MdctNum = data[0].MdctNum.Trim();
            obj.OrderDt = data[0].OrderDt.Trim();
            obj.OrderDtm = data[0].OrderDtm.Trim();
            obj.OrderNum = this.OrderNum.Trim();
            obj.RoomNum = isNullZero(data[0].RoomNum).Trim();
            obj.BedNum = data[0].BedNum.Trim();
            obj.PtntNum = data[0].PtntNum.Trim();
            obj.AllergyNm = data[0].AllergyNm.Trim();
            obj.Note = data[0].Note.Trim();
            obj.PtntNm = data[0].PtntNm.Trim();
            obj.Sex = data[0].Sex.Trim();
            obj.DoctorNm = data[0].DoctorNm.Trim();
            obj.Birthday = String.Format("{0:yyyyMMdd}", data[0].Birthday).Trim(); 

            List<MedItem> MedItem = new List<MedItem>();
            MedItem item;
            MedItemDose m;

            for (int i = 0; i < data.Count; i++) {
                item = new MedItem();
                
                item.MedCd = data[i].MedCd.Trim();
                item.MedNm = data[i].MedNm.Trim();
                item.MedNote = data[i].MedNote.Trim();
                item.MedNote2 = data[i].MedNote2.Trim();
                item.MedSpec = data[i].MedSpec.Trim();
                item.MedUnit = data[i].MedUnit.Trim();
                item.UseAtcYn = data[i].UseAtcYn.Trim();

                m = new MedItemDose();
                m.DoseList = data[i].DoseList.Trim();
                m.TakeDays = data[i].TakeDays.Trim();
                m.TakeDt = data[i].TakeDt.Trim();
 
                item.MedItemDose = m;
                MedItem.Add(item);
            }

            obj.med = MedItem.ToArray();

            this.xmlMessage = MySerializer<OrderInfo>.Serialize(obj);
            this.xmlMessage = this.xmlMessage.Replace("<med>\r\n    ", "").Replace("</med>\r\n", "");
            return xmlMessage;
        }

        public bool exportFile()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(this.xmlMessage);
            XmlNode node = xmlDocument.GetElementsByTagName("OrderNum")[0];
            string OrderNum = node.InnerXml.ToString();

            if (createFile(this.exportPath+@"\"+ OrderNum+".xml", this.xmlMessage))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string getOrderNum()
        {
            if (this.dateNow != getDateNow())
            {
                this.dateNow = getDateNow();
                this.runningNum = 1;
            }
            else
            {
                this.runningNum += 1;
            }

            this.OrderNum = this.OrderHeader + String.Format("{0:D5}", this.runningNum);
            return this.OrderNum;
        }

        private bool createFile(string fullfileName,string text)
        {
            FileInfo fi = new FileInfo(fullfileName);
            if (!File.Exists(fi.FullName))
            {
                try
                {
                    //Encoding.GetEncoding("iso-8859-1")
                    using (var sw = new StreamWriter(File.Open(fi.FullName, FileMode.CreateNew), Encoding.UTF8))
                    {
                        sw.WriteLine(text);
                        Console.WriteLine("OK!. Create file " + fi.FullName + " complete.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error!. Create file " + fi.FullName + "not complete.");
                    Console.WriteLine(ex.ToString());
                    return true;
                }
            }else
            {
                return false;
            }
        }

        private string isNullZero(string data)
        {
            string result = "0";
            if(data=="" || data == null)
            {
                result = "0";
            }
            else
            {
                result = data;
            }
            return result;
        }

        private static Random random = new Random();

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string getDateNow()
        {
            string result = "";
            DateTime dateTime = DateTime.UtcNow.Date;
            result = dateTime.ToString("yyyyMMdd");
            return result;
        }

    }

    public class IPD
    {
        //<OrderInfo>
        public string HsptCd; // 1
        public string DptmtCd; // รหัสแผนก
        public string WardCd; // รหัสวอร์ด EX: 441
        public string DataClsf; // N
        public string InOutClsf; //I,O
        public string MdctNum; //เลขที่ใบสั่ง 65017793
        public string OrderDt; // 20220304
        public string OrderDtm; // 20220304074709
        public string OrderNum; // LJ4CN00002
        public string RoomNum; // 02 เลขที่ห้อง
        public string BedNum;   //เลขที่เตียง
        public string PtntNum; // 1324911
        public string AllergyNm; // ใช้แสดงข้อมูล AN สำหรับผู้ป่วยใน
        public string Note; // อื่นๆ
        public string PtntNm; // นาย สาย ขันธะวินะหุ
        public string Sex; // M,F
        public string DoctorNm; //รหัสหมอสั่ง
        public string Birthday; // 1943-11-30 00:00:00.0
                                //</OrderInfo>
                                //<MedItem>
        public string MedCd; //DOXA2
        public string MedNm; // Doxazosine 2 mg.Tab.
        public string MedNote; // ก่อนนอน
        public string MedNote2; // คำเตือนการใช้ยา
        public string MedSpec; // 2mg
        public string MedUnit; //TAB
        public string UseAtcYn; //Y
                                //<MedItemDose>
        public string DoseList; //2000:1 (เวลา:จำนวนเม็ด)
        public string TakeDays; // 1 (จ่าย1วัน)
        public string TakeDt; // 20220304 วันที่จ่าย
                              //</MedItemDose>
                              //</MedItem>
    }

    public class OrderInfo
    {
        public string HsptCd { get; set; }
        public string DptmtCd { get; set; }
        public string WardCd { get; set; }
        public string DataClsf { get; set; }
        public string InOutClsf { get; set; }
        public string MdctNum { get; set; }
        public string OrderDt { get; set; }
        public string OrderDtm { get; set; }
        public string OrderNum { get; set; }
        public string RoomNum { get; set; }
        public string BedNum { get; set; }
        public string PtntNum { get; set; }
        public string AllergyNm { get; set; }
        public string Note { get; set; }
        public string PtntNm { get; set; }
        public string Sex { get; set; }
        public string DoctorNm { get; set; }
        public string Birthday { get; set; }
        public MedItem[] med { get; set; }
    }

    public class MedItem
    {
        public string MedCd { get; set; }
        public string MedNm { get; set; }
        public string MedNote { get; set; }
        public string MedNote2 { get; set; }
        public string MedSpec { get; set; }
        public string MedUnit { get; set; }
        public string UseAtcYn { get; set; }
        public MedItemDose MedItemDose { get; set; }
    }

    public class MedItemDose
    {
        public string DoseList { get; set; }
        public string TakeDays { get; set; }
        public string TakeDt { get; set; }
    }

    public class MySerializer<T> where T : class
    {
        public static string Serialize(T obj)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
            using (var sww = new Utf8StringWriter())
            {
                using (XmlTextWriter writer = new XmlTextWriter(sww) { Formatting = Formatting.Indented })
                {
                    xsSubmit.Serialize(writer, obj);
                    return sww.ToString();
                }
            }
        }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

}
