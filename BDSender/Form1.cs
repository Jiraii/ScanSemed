using BDSender.forms;
using CrystalDecisions.CrystalReports.Engine;
using gd4lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using CrystalDecisions.Shared;
using System.Net.NetworkInformation;
using BDSender.cls;
using System.Globalization;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using System.IO.Ports;
using static gd4lib.dih;
using Newtonsoft.Json;

namespace BDSender
{
    public partial class frm_main : Form
    {
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21; private void txtSearch_TextChanged(object sender, EventArgs e) {}        
        System.Drawing.Color colFloor;
        string outp = Properties.Settings.Default.OUTPUT_LR;
        int waittime = Convert.ToInt16(Properties.Settings.Default.WAIT_TIME);
        string windowNo = "";
        //private BDSender.forms.frmNumpad frmNum;
        frm_Confirm frmconfirm;
        frm_yesno frmyesno; 
        clsconvertdate clsconvertdate = new clsconvertdate();
        public gd4lib.utils ut;
        public gd4lib.dih dih;
        dih_webserv.DIHPMPFWebservice dihapi;
        module.cls_meddispense md_med;
        module.cls_semed md_se;
        static SerialPort _serialPort;
        DataTable tb_order;
        DataTable tb_OutpItem;
       
        public frm_main()
        {
            InitializeComponent();
            colFloor = System.Drawing.ColorTranslator.FromHtml("#ede6e0");
            
            dih = new dih();
            ut = new utils();
          

        }

        private void frm_main_Load(object sender, EventArgs e)
        {
            try {
                this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
                this.webView21.Dock = System.Windows.Forms.DockStyle.Fill;
                this.Controls.Add(this.webView21);
                this.webView21.BringToFront();
                this.webView21.EnsureCoreWebView2Async(null);
                this.webView21.CoreWebView2InitializationCompleted += WebView21_CoreWebView2InitializationCompleted;
                this.webView21.WebMessageReceived += WebView21_WebMessageReceived;
            } catch(Exception exWeb) { Console.WriteLine("WebView2 Init Failed: " + exWeb.ToString()); System.IO.File.WriteAllText("webview2_error.txt", exWeb.ToString()); }
            try {
                Class1._serialPort = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
                Class1._serialPort.Handshake = Handshake.None;
                Class1._serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler_);
                Class1._serialPort.Open();
            } catch (Exception exCOM) {
                Console.WriteLine("COM3 Failed: " + exCOM.Message);
            }

            if (Class1._serialPort == null)
            {
                try
                {
                    Class1._serialPort = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
                    Class1._serialPort.Handshake = Handshake.None;
                    Class1._serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler_);
                    Class1._serialPort.Open();

                    Console.WriteLine("Serial Port Opened.");
                    lbRfidStatus.Text = "Serial Port Opened.";

                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("พอร์ต COM3 ถูกใช้งานอยู่ หรือไม่มีสิทธิ์เข้าถึง\n\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
            else if (Class1._serialPort != null && !Class1._serialPort.IsOpen)
            {
                try { Class1._serialPort.Open(); } catch (Exception ex) { Console.WriteLine("Port error: " + ex.Message); }
            }
            else if (Class1._serialPort.IsOpen)
            {
                Console.WriteLine("Serial Port Already Opened.");
                lbRfidStatus.Text = "Serial Port Already Opened.";
            }
            else
            {
                lbRfidStatus.Text = "Serial Port error";
            }


            txtSearch.Focus();
            dihapi = new dih_webserv.DIHPMPFWebservice();
            md_med = new module.cls_meddispense();
            md_se = new module.cls_semed();

            //tb_order = md_se.get_order(outp);
            //dgvOrder.DataSource = tb_order;

            this.BackColor = colFloor;
            tableLayoutPanel2.BackColor = colFloor;
            if (outp.ToUpper() == "L")
            {
                ucFrm_Outp1.setOutp("1");
                ucFrm_Outp2.setOutp("2");
                windowNo = "1";
            }
            else if (outp.ToUpper() == "R")
            {
                ucFrm_Outp1.setOutp("3");
                ucFrm_Outp2.setOutp("4");
                windowNo = "2";
            }
            else
            {
                ucFrm_Outp1.setOutp("1");
                ucFrm_Outp2.setOutp("2");
                windowNo = "0";
            }

            lblVersion.Text = "Version : " + BDSender.Properties.Settings.Default.Version;
           
            txtSearch.Focus();

        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string rfid = "";
                string data = _serialPort.ReadExisting();
                txtSearch.Invoke(new MethodInvoker(delegate {
                    if (data.Split('|')[2] != "0")
                    {
                        txtSearch.Text = data.Split('|')[2];
                        //Class1.Main_Prog();
                        BarcodeBasket();
                    }

                }));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Serial read error: " + ex.Message);
            }
        }
        private void DataReceivedHandler_(object sender, SerialDataReceivedEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate 
            {
                try
                {     
                    Class1.Main_Prog();
                    BarcodeBasket();
                   
                }
                catch (Exception ex)
                {
                    //lbRfidStatus.Text = ex.Message.ToString();
                    Console.WriteLine("Serial read error: " + ex.Message);
                    //label6.Text = "Serial read error: " + ex.Message;
                }
            }));
        }
        public async void BarcodeBasket()
        {
            this.MaximumSize = this.Size;
            this.AutoScaleMode = AutoScaleMode.None;

            if (Class1.data.Trim() != "0")
            {
                if (Class1.data.Trim().Length == 16)
                {
                    txtSearch.Text = Class1.data.Trim().ToUpper();
                    //await autoGenPackage();
                    autoGenPackageAPI(Class1.data.Trim().ToUpper());
                    clearTextBox_search("");
                }
                else
                {
                    txtSearch.Focus();
                    txtSearch.Clear();
                }

            }

        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            //DataTable tb;

            if (e.KeyChar == (char)13)
            {
                //MessageBox.Show(" Enter pressed ");
                //tb = md_med.get_OrderItem(txtSearch.Text);
                //if (txtSearch.Text == "0000")
                //{
                //    this.Close();
                //}
                //txtSearch.Text = txtSearch.Text.ToUpper();
                ////await autoGenPackage();
                //autoGenPackageAPI(txtSearch.Text);
                //clearTextBox_search("");
                //txtSearch.Text = "";
                //txtSearch.Focus();
            }            
        }

        private void txtSearch_Click(object sender, EventArgs e)
        {
            //frmconfirm = new frm_Confirm("ไม่พบข้อมูลในระบบที่ต้องจัด");
            //frmconfirm.StartPosition = FormStartPosition.CenterScreen;
            //frmconfirm.Show();
            //frmNum = new frmNumpad("กรอกเลขตะกร้า", ref txtSearch);
            //if (frmNum.ShowDialog() == DialogResult.Yes)
            //{
            //    autoGenPackage();
            //}
            //autoGenPackage();
            //PrintSlip("6727000063");
        }

        //private async Task<bool> autoGenPackage()
        public List<object> GenJson_Regisbasket(string datetime)
        {
            List<object> listpackagemaster = new List<object>();
            List<object> listJson = new List<object>();
            try
            {
                if (cls.orderDetail.db_packagemaster.Rows.Count > 0)
                {

                    List<object> listpack = new List<object>();

                    foreach (DataRow rw in cls.orderDetail.db_packagemaster.Rows)
                    {
                        listpackagemaster.Add(new
                        {
                            _id = rw["_id"].ToString(),                            
                            bddate = "datetime",
                        });

                    }

                }
                return listpackagemaster;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return listpackagemaster;
            }
            finally
            {

            }
        }

        public async void autoGenPackageAPI(string rfid)
        {
            string dihapi_result = "";
            string XML2DIH_OPD = "";
            bool result ;
            int Code;
            string orderNo = "";
            DataTable dt_Order = new DataTable();
            string[] drugcode;
            List<object> ListJson = new List<object>();
            //System.Threading.Thread.Sleep(5000);
            if (rfid != "")
            {
                await cls.cls_service_api.RequestPackagemasterSEmed(rfid);

                List<OPD> data = new List<OPD>();
                gd4lib.OPD d;
                DialogResult frm_confirm_allergy = DialogResult.Yes;
                DialogResult frm_confirm = DialogResult.No;
                if (cls.orderDetail.db_data.Rows.Count > 0)
                {
                    
                    if (cls.orderDetail.db_packagemaster.Rows.Count > 0)
                    {
                        //if(cls.orderDetail.db_packagemaster.Rows[0]["bddate"].ToString() == "")
                        //{

                        //}
                        //else
                        //{
                        //    frmyesno = new frm_yesno(" จัดยาแล้ว \r\n");
                        //    frmyesno.StartPosition = FormStartPosition.CenterScreen;
                        //    frmyesno.Show();
                        //    txtSearch.Text = "";
                        //    txtSearch.Focus();
                        //}

                        string select = "";
                        DataTable dtg_drug = new DataTable();
                        //dtg_drug = cls.orderDetail.db_packagemaster.AsEnumerable().GroupBy(r => r.Field<string>("orderitemcode")).Select(g => g.OrderBy(r => r["orderitemcode"]).First()).CopyToDataTable();
                        dtg_drug = cls.orderDetail.db_packagemaster
                        .AsEnumerable()
                        .Where(r => r.Field<string>("shelfzone") == "SE-MED") // เงื่อนไขเฉพาะ SE-MED
                        .GroupBy(r => new
                        {
                            ShelfZone = r.Field<string>("shelfzone"),
                            OrderItemCode = r.Field<string>("orderitemcode")
                        })
                        .Select(g => g.OrderBy(r => r["orderitemcode"]).First())
                        .CopyToDataTable();


                        drugcode = new string[dtg_drug.Rows.Count];
                        for (int r = 0; r < dtg_drug.Rows.Count; r++)
                        {
                            drugcode[r] = dtg_drug.Rows[r]["orderitemcode"].ToString();
                        }


                        string arraydrug = "[\"" + string.Join("\",\"", drugcode) + "\"]";

                        DataTable dt_stock = new DataTable();
                        dt_stock = await cls.cls_service_api.Request_Getsemedstock(arraydrug);

                        if (dt_stock.Rows.Count > 0)
                        {
                            // ดึงรายการ order ที่ไม่ซ้ำกันตาม orderitemcode
                            DataTable dtg_PresSub = cls.orderDetail.db_packagemaster.AsEnumerable().Where(r => r.Field<string>("shelfzone") == "SE-MED") // เงื่อนไขเฉพาะ SE-MED
                        .GroupBy(r => new
                        {
                            ShelfZone = r.Field<string>("shelfzone"),
                            OrderItemCode = r.Field<string>("orderitemcode")
                        })
                        .Select(g => g.OrderBy(r => r["orderitemcode"]).First())
                        .CopyToDataTable();


                            string checkStock = "";
                            int total = 0, orderqty = 0;

                            foreach (DataRow r in dtg_PresSub.Rows)
                            {
                                string orderItemCode = r["orderitemcode"]?.ToString();
                                string orderItemName = r["orderitemname"]?.ToString();
                                string orderUnitCode = r["orderunitcode"]?.ToString();

                                // แปลงค่า orderqty อย่างปลอดภัย
                                if (!int.TryParse(r["orderqty"]?.ToString(), out orderqty))
                                    continue;

                                // filter รายการใน stock ให้ตรงกับ drugCode
                                DataRow[] results = dt_stock.Select($"drugCode = '{orderItemCode}'");

                                if (results.Length > 0 && int.TryParse(results[0]["Quantity"]?.ToString(), out total))
                                {
                                    total = total * Convert.ToInt32(results[0]["packageRatio"]?.ToString());
                                    if (total < orderqty)
                                    {
                                        if (string.IsNullOrEmpty(checkStock))
                                        {
                                            checkStock = "ยาไม่พอจ่าย\r\n";
                                        }
                                        checkStock += $"{orderItemName} = {orderqty - total} {orderUnitCode}\r\n";
                                    }
                                }
                                else
                                {
                                    // กรณีไม่เจอ stock
                                    if (string.IsNullOrEmpty(checkStock))
                                    {
                                        checkStock = "ยาไม่พอจ่าย\r\n";
                                    }
                                    checkStock += $"{orderItemName} ไม่มีข้อมูลในคลัง\r\n";
                                }
                            }

                            if (checkStock == "")
                            {
                                if (frm_confirm_allergy == DialogResult.Yes)
                                {
                                    #region makeData and send
                                    foreach (DataRow row in dtg_PresSub.Rows)
                                    {

                                        #region สร้างข้อมูลส่งเครื่อง
                                        d = new OPD();
                                        d.patID = cls.orderDetail.db_data.Rows[0]["hn"].ToString();  // 1
                                        d.patName = cls.orderDetail.db_data.Rows[0]["patientname"].ToString().Replace("/", "").Replace("'", "");  // ชื่อ-นามสกุล
                                        d.gender = cls.orderDetail.db_data.Rows[0]["sex"].ToString(); // เพศ
                                        d.birthday = clsconvertdate.convert_en(cls.orderDetail.db_data.Rows[0]["patientdob"].ToString()); // วันเกิด
                                        d.QN = cls.orderDetail.db_data.Rows[0]["qn"].ToString(); //เลขคิว
                                        d.AN = cls.orderDetail.db_data.Rows[0]["hn"].ToString(); // เลขการรักษา

                                        if (cls.orderDetail.db_data.Rows[0]["patientdob"].ToString() != "")
                                        {
                                            string year = "";
                                            string yearNow = "";

                                            yearNow = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                                            var date = cls.orderDetail.db_data.Rows[0]["patientdob"].ToString();
                                            var yNow = yearNow;

                                            year = clsconvertdate.convert_en_y(date);
                                            yearNow = clsconvertdate.convertdate_YYYY_EN(yNow);

                                            if (year != "" && yearNow != "")
                                            {
                                                d.age = (Convert.ToInt32(yearNow) - Convert.ToInt32(year)).ToString();
                                            }
                                            else
                                            {
                                                d.age = "";
                                            }
                                        }
                                        else
                                        {
                                            d.age = "";
                                        }


                                        d.identity = ""; // 
                                        d.insuranceNo = ""; // 
                                        d.chargeType = ""; // 
                                        d.orderNo = row["prescriptionno_sup"].ToString();   // เลขที่ Order
                                        orderNo = row["prescriptionno_sup"].ToString();
                                        d.orderType = ""; // 
                                        d.pharmacy = "OPD"; // ห้องจัดยา
                                        d.windowNo = windowNo; // ฝั่งที่จ่าย
                                        d.paymentIP = ""; // 
                                        d.paymentDT = clsconvertdate.convert_en_time(cls.orderDetail.db_data.Rows[0]["ordercreatedate"].ToString()); // วันที่จ่าย
                                        d.outpNo = ""; //เลขที่ช่องจ่าย
                                        d.visitNo = cls.orderDetail.db_data.Rows[0]["hn"].ToString(); // เลขที่การรักษา
                                        d.deptCode = cls.orderDetail.db_data.Rows[0]["wardcode"].ToString(); //รหัสแผนก
                                        d.deptName = "";/*cls.orderDetail.db_data.Rows[0]["wardname"].ToString();*/ // ชื่อแผนก
                                        d.doctCode = cls.orderDetail.db_data.Rows[0]["doctorcode"].ToString(); // รหัสหมอ
                                        d.doctName = cls.orderDetail.db_data.Rows[0]["doctorname"].ToString(); // ชื่อหมอ
                                        d.diagnosis = ""; // 
                                        d.alias = ""; //
                                        d.code = row["orderitemcode"].ToString().Replace("/", "").Replace("'", "");  // รหัสยาที่จ่าย
                                        d.name = row["orderitemname"].ToString().Replace("/", "").Replace("'", "");  // ชื่อยาที่จ่าย

                                        select = string.Format("orderitemcode = '{0}'", row["orderitemcode"]);
                                        DataRow[] results = dt_stock.Select();
                                        if (results[0]["spec"].ToString() != "")
                                        {
                                            d.spec = results[0]["spec"].ToString();
                                        }
                                        else
                                        {
                                            d.spec = "N/A";
                                        }

                                        d.firmName = "NKP"; // ผู้ผลิตยา
                                        d.qty = row["orderqty"].ToString(); // จำนวนที่จ่าย
                                        d.unit = row["orderunitcode"].ToString(); // หน่วยที่จ่าย
                                        d.method = ""; // จำนวนเม็ดที่ทาน
                                        d.type = ""; // 
                                        d.note = row["shelfzone"].ToString(); // 1 (จ่าย1วัน)
                                        d.itemNo = ""; // 20220304 วันที่จ่าย

                                        data.Add(d);
                                        #endregion
                                        // }
                                    }

                                    dataGridView1.Rows.Add(cls.orderDetail.db_data.Rows[0]["vn"].ToString());

                                    try
                                    {
                                        XML2DIH_OPD = dih.genXML2_OPD(data);

                                        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

                                        await cls.cls_service_api.update_resultSemed(json);

                                        //dihapi_result = dihapi.outpOrderDispense(XML2DIH_OPD);// ใช้ส่งเข้าเครื่องจริง

                                        //result = dihapi.outpOrderDispense(XML2DIH_OPD);
                                        //ut.log("ส่งข้อมูลเข้า PMPF จ่าย SE สำเร็จ");

                                        dihapi_result = "<result><status><code>0</code><message></message></status></result>";
                                    }
                                    catch
                                    {
                                        frmconfirm = new frm_Confirm("ไม่สามารถส่งข้อมูลเข้าเครื่องได้");
                                        frmconfirm.StartPosition = FormStartPosition.CenterScreen;
                                        frmconfirm.Show();
                                        result = false;
                                    }

                                    XmlSerializer serializer = new XmlSerializer(typeof(Result));
                                    using (TextReader reader = new StringReader(dihapi_result))
                                    {
                                        Result rs = (Result)serializer.Deserialize(reader);
                                        Code = rs.Status.Code;
                                        if (Code == 0)
                                        {
                                            txtSearch.Text = "";
                                            txtSearch.Focus();
                                            System.Threading.Thread.Sleep(5000);
                                            PrintSlipAsync(orderNo);
                                            ListJson = GenJson_Regisbasket("");

                                            if (ListJson.Count > 0)
                                            {
                                                await cls_service_api.update_regisbasket(ListJson);
                                            }
                                        }
                                        else
                                        {
                                            txtSearch.Text = "";
                                            txtSearch.Focus();
                                            frmyesno = new frm_yesno("ไม่สามารถส่งข้อมูลเข้าเครื่องได้");
                                            frmyesno.StartPosition = FormStartPosition.CenterScreen;
                                            frmyesno.Show();
                                        }
                                    }


                                    #endregion
                                }
                            }
                            else
                            {
                                frmconfirm = new frm_Confirm(checkStock);
                                frmconfirm.StartPosition = FormStartPosition.CenterScreen;
                                frmconfirm.Show();
                                if (frm_confirm == DialogResult.Yes)
                                {
                                    #region makeData and send
                                    foreach (DataRow row in dtg_PresSub.Rows)
                                    {

                                        #region สร้างข้อมูลส่งเครื่อง
                                        d = new OPD();
                                        d.patID = cls.orderDetail.db_data.Rows[0]["hn"].ToString();  // 1
                                        d.patName = cls.orderDetail.db_data.Rows[0]["patientname"].ToString().Replace("/", "").Replace("'", "");  // ชื่อ-นามสกุล
                                        d.gender = cls.orderDetail.db_data.Rows[0]["sex"].ToString(); // เพศ
                                        d.birthday = clsconvertdate.convert_en(cls.orderDetail.db_data.Rows[0]["patientdob"].ToString()); // วันเกิด
                                        d.QN = cls.orderDetail.db_data.Rows[0]["qn"].ToString(); //เลขคิว
                                        d.AN = cls.orderDetail.db_data.Rows[0]["hn"].ToString(); // เลขการรักษา

                                        if (cls.orderDetail.db_data.Rows[0]["patientdob"].ToString() != "")
                                        {
                                            string year = "";
                                            string yearNow = "";

                                            yearNow = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                                            var date = cls.orderDetail.db_data.Rows[0]["patientdob"].ToString();
                                            var yNow = yearNow;

                                            year = clsconvertdate.convert_en_y(date);
                                            yearNow = clsconvertdate.convertdate_YYYY_EN(yNow);

                                            if (year != "" && yearNow != "")
                                            {
                                                d.age = (Convert.ToInt32(yearNow) - Convert.ToInt32(year)).ToString();
                                            }
                                            else
                                            {
                                                d.age = "";
                                            }
                                        }
                                        else
                                        {
                                            d.age = "";
                                        }


                                        d.identity = ""; // 
                                        d.insuranceNo = ""; // 
                                        d.chargeType = ""; // 
                                        d.orderNo = row["prescriptionno_sup"].ToString();   // เลขที่ Order
                                        orderNo = row["prescriptionno_sup"].ToString();
                                        d.orderType = ""; // 
                                        d.pharmacy = "OPD"; // ห้องจัดยา
                                        d.windowNo = windowNo; // ฝั่งที่จ่าย
                                        d.paymentIP = ""; // 
                                        d.paymentDT = clsconvertdate.convert_en_time(cls.orderDetail.db_data.Rows[0]["ordercreatedate"].ToString()); // วันที่จ่าย
                                        d.outpNo = ""; //เลขที่ช่องจ่าย
                                        d.visitNo = cls.orderDetail.db_data.Rows[0]["hn"].ToString(); // เลขที่การรักษา
                                        d.deptCode = cls.orderDetail.db_data.Rows[0]["wardcode"].ToString(); //รหัสแผนก
                                        d.deptName = "";/*cls.orderDetail.db_data.Rows[0]["wardname"].ToString();*/ // ชื่อแผนก
                                        d.doctCode = cls.orderDetail.db_data.Rows[0]["doctorcode"].ToString(); // รหัสหมอ
                                        d.doctName = cls.orderDetail.db_data.Rows[0]["doctorname"].ToString(); // ชื่อหมอ
                                        d.diagnosis = ""; // 
                                        d.alias = ""; //
                                        d.code = row["orderitemcode"].ToString().Replace("/", "").Replace("'", "");  // รหัสยาที่จ่าย
                                        d.name = row["orderitemname"].ToString().Replace("/", "").Replace("'", "");  // ชื่อยาที่จ่าย

                                        select = string.Format("orderitemcode = '{0}'", row["orderitemcode"]);
                                        DataRow[] results = dt_stock.Select();
                                        if (results[0]["spec"].ToString() != "")
                                        {
                                            d.spec = results[0]["spec"].ToString();
                                        }
                                        else
                                        {
                                            d.spec = "N/A";
                                        }

                                        d.firmName = "NKP"; // ผู้ผลิตยา
                                        d.qty = row["orderqty"].ToString(); // จำนวนที่จ่าย
                                        d.unit = row["orderunitcode"].ToString(); // หน่วยที่จ่าย
                                        d.method = ""; // จำนวนเม็ดที่ทาน
                                        d.type = ""; // 
                                        d.note = row["shelfzone"].ToString(); // 1 (จ่าย1วัน)
                                        d.itemNo = ""; // 20220304 วันที่จ่าย

                                        data.Add(d);
                                        #endregion
                                        // }
                                    }

                                    dataGridView1.Rows.Add(cls.orderDetail.db_data.Rows[0]["vn"].ToString());

                                    try
                                    {
                                        XML2DIH_OPD = dih.genXML2_OPD(data);

                                        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

                                        await cls.cls_service_api.update_resultSemed(json);

                                        //dihapi_result = dihapi.outpOrderDispense(XML2DIH_OPD);// ใช้ส่งเข้าเครื่องจริง

                                        //result = dihapi.outpOrderDispense(XML2DIH_OPD);
                                        //ut.log("ส่งข้อมูลเข้า PMPF จ่าย SE สำเร็จ");

                                        dihapi_result = "<result><status><code>0</code><message></message></status></result>";
                                    }
                                    catch
                                    {
                                        frmconfirm = new frm_Confirm("ไม่สามารถส่งข้อมูลเข้าเครื่องได้");
                                        frmconfirm.StartPosition = FormStartPosition.CenterScreen;
                                        frmconfirm.Show();
                                        result = false;
                                    }

                                    XmlSerializer serializer = new XmlSerializer(typeof(Result));
                                    using (TextReader reader = new StringReader(dihapi_result))
                                    {
                                        Result rs = (Result)serializer.Deserialize(reader);
                                        Code = rs.Status.Code;
                                        if (Code == 0)
                                        {
                                            txtSearch.Text = "";
                                            txtSearch.Focus();
                                            System.Threading.Thread.Sleep(5000);
                                            PrintSlipAsync(orderNo);
                                        }
                                        else
                                        {
                                            txtSearch.Text = "";
                                            txtSearch.Focus();
                                            frmyesno = new frm_yesno("ไม่สามารถส่งข้อมูลเข้าเครื่องได้");
                                            frmyesno.StartPosition = FormStartPosition.CenterScreen;
                                            frmyesno.Show();
                                        }
                                    }


                                    #endregion
                                }
                            }
                        }
                        else
                        {

                            frmyesno = new frm_yesno(" ยาไม่มีในตู้ \r\n");
                            frmyesno.StartPosition = FormStartPosition.CenterScreen;
                            frmyesno.Show();
                            //txtSearch.Text = "";
                            //txtSearch.Focus();
                        }

                    }
                    else
                    {

                        frmyesno = new frm_yesno(" ไม่มียาที่ต้องจัด หรือ ไม่ได้ปริ้นสติ๊กเกอร์ \r\n");
                        frmyesno.StartPosition = FormStartPosition.CenterScreen;
                        frmyesno.Show();
                        //txtSearch.Text = "";
                        //txtSearch.Focus();

                    }
                }
                else
                {

                    frmyesno = new frm_yesno(" ไม่มีรายการที่ต้องจัด \r\n");
                    frmyesno.StartPosition = FormStartPosition.CenterScreen;
                    frmyesno.Show();
                    //txtSearch.Text = "";
                    //txtSearch.Focus();
                }

            }


        }
        public async void API_get_OrderItem(string rfid)
        {
            cls.orderDetail.db_drug = new DataTable();
            cls.orderDetail.db_data = new DataTable();
            cls.orderDetail.db_packagemaster = new DataTable();
            await cls.cls_service_api.RequestPackagemasterSEmed(rfid);
        }
        private void autoGenPackage()
        {
            string XML2DIH_OPD = "";
            string result = "";
            int Code;
            string orderNo = "";

            DataTable dt_Order = md_med.get_OrderItem(txtSearch.Text.ToUpper());
            //cls_service_api.RequestPackagemasterReprint(txtSearch.Text.ToUpper());
            DataTable dt_stock;
            List<OPD> data = new List<OPD>();
            //List<string> drugCodeList = new List<string>();
            gd4lib.OPD d;
            DialogResult frm_confirm_allergy = DialogResult.Yes;
            if (dt_Order.Rows.Count > 0)
            {
                List<string> drugCodeList = dt_Order.AsEnumerable()
                           .Select(r => r.Field<string>("orderitemcode"))
                           .ToList();
                dt_stock = md_se.get_semedStock(drugCodeList);
                string checkStock = "";
                string select = "";
                int total, orderqty = 0;
                foreach (DataRow r in dt_Order.Rows)
                {
                    orderqty = Convert.ToInt16(r["orderqty"]);
                    select = string.Format("orderitemcode = '{0}'", r["orderitemcode"]);
                    DataRow[] results = dt_stock.Select();
                    total = Convert.ToInt16(results[0]["total"]);
                    if (total < orderqty)
                    {
                        if (checkStock == "")
                        {
                            checkStock = "ยาไม่พอจ่าย\r\n";
                        }
                        checkStock += string.Format("{0} = {1} {2}\r\n", r["orderitemname"].ToString(),orderqty-total, r["orderunitcode"]);
                    }
                }

                if (checkStock == "")
                {
                    if (frm_confirm_allergy == DialogResult.Yes)
                    {
                        #region makeData and send
                        foreach (DataRow row in dt_Order.Rows)
                        {
                            #region สร้างข้อมูลส่งเครื่อง
                            d = new OPD();
                            d.patID = row["hn"].ToString();  // 1
                            d.patName = row["patientname"].ToString(); // ชื่อ-นามสกุล
                            d.gender = row["sex"].ToString(); // เพศ
                            d.birthday = row["patientdob"].ToString(); // วันเกิด
                            d.QN = row["queue"].ToString(); //เลขคิว
                            d.AN = row["hn"].ToString(); // เลขการรักษา
                            d.age = row["age"].ToString(); // อายุ
                            d.identity = ""; // 
                            d.insuranceNo = ""; // 
                            d.chargeType = ""; // 
                            d.orderNo = row["prescriptionno_sup"].ToString();   // เลขที่ Order
                            orderNo = row["prescriptionno_sup"].ToString();
                            d.orderType = ""; // 
                            d.pharmacy = "OPD"; // ห้องจัดยา
                            d.windowNo = windowNo; // ฝั่งที่จ่าย
                            d.paymentIP = ""; // 
                            d.paymentDT = row["ordercreatedate"].ToString(); // วันที่จ่าย
                            d.outpNo = ""; //เลขที่ช่องจ่าย
                            d.visitNo = row["an"].ToString(); // เลขที่การรักษา
                            d.deptCode = row["wardcode"].ToString(); //รหัสแผนก
                            d.deptName = row["wardname"].ToString(); // ชื่อแผนก
                            d.doctCode = row["doctorcode"].ToString(); // รหัสหมอ
                            d.doctName = row["doctorname"].ToString(); // ชื่อหมอ
                            d.diagnosis = ""; // 
                            d.alias = ""; //
                            d.code = row["orderitemcode"].ToString(); // รหัสยาที่จ่าย
                            d.name = row["orderitemname"].ToString(); // ชื่อยาที่จ่าย
                            d.spec = row["Strength"].ToString(); // ความแรงยา
                            d.firmName = row["firmname"].ToString(); // ผู้ผลิตยา
                            d.qty = row["orderqty"].ToString(); // จำนวนที่จ่าย
                            d.unit = row["orderunitcode"].ToString(); // หน่วยที่จ่าย
                            d.method = ""; // จำนวนเม็ดที่ทาน
                            d.type = ""; // 
                            d.note = row["shelfzone"].ToString(); // 1 (จ่าย1วัน)
                            d.itemNo = ""; // 20220304 วันที่จ่าย

                            data.Add(d);
                            #endregion
                            // }
                        }
                        try
                        {
                            XML2DIH_OPD = dih.genXML2_OPD(data);
                            result = dihapi.outpOrderDispense(XML2DIH_OPD);
                            //ut.log("ส่งข้อมูลเข้า PMPF จ่าย SE สำเร็จ");
                        }
                        catch
                        {
                            frmconfirm = new frm_Confirm("ไม่สามารถส่งข้อมูลเข้าเครื่องได้");
                            frmconfirm.StartPosition = FormStartPosition.CenterScreen;
                            frmconfirm.Show();
                            result = "<result><status><code>1</code><message>ไม่สามารถส่งข้อมูลเข้าเครื่องได้</message></status></result>";
                        }

                        // ใช้ส่งเข้าเครื่องจริง
                        //  result = "<result><status><code>0</code><message></message></status></result>"; //ใช้เทส
                        XmlSerializer serializer = new XmlSerializer(typeof(Result));
                        using (TextReader reader = new StringReader(result))
                        {
                            Result rs = (Result)serializer.Deserialize(reader);
                            Code = rs.Status.Code;
                            if (Code == 0)
                            {
                                txtSearch.Text = "";
                                txtSearch.Focus();
                                System.Threading.Thread.Sleep(5000);
                                PrintSlipAsync(orderNo);
                            }
                            else
                            {
                                txtSearch.Text = "";
                                txtSearch.Focus();
                            }
                        }
                        #endregion
                    }
                }else
                {
                    frmconfirm = new frm_Confirm(checkStock);
                    frmconfirm.StartPosition = FormStartPosition.CenterScreen;
                    frmconfirm.Show();
                    //txtSearch.Text = "";
                    //txtSearch.Focus();
                }
            }
            else
            {
                frmconfirm = new frm_Confirm("ไม่พบข้อมูลในระบบที่ต้องจัด");
                frmconfirm.StartPosition = FormStartPosition.CenterScreen;
                frmconfirm.Show();
                //txtSearch.Text = "";
                //txtSearch.Focus();
            }                  
        }

        public void clearTextBox_search(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(clearTextBox_search), new object[] { value });
                return;
            }
            txtSearch.Text = "";
        }

        private async void timer_wait_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("timer_wait");
            txtSearch.Focus();
            //tb_order = md_se.get_order(outp.ToUpper());

            string Index = "";
            if (outp.ToUpper() == "L")
            {
                Index = "1";
            }
            else if (outp.ToUpper() == "R")
            {
                Index = "2";
            }
            else
            {
                Index = "1";
            }
            tb_order = await cls.cls_service_api.Request_Getorder(Index);
            //dgvOrder.DataSource = tb_order;
        }

        private async void timer_outp_status_Tick(object sender, EventArgs e)
        {
            txtSearch.Focus();

            //Console.WriteLine("timer_outp_status");
            //tb_OutpItem = md_se.get_OrderItem(outp.ToUpper());

            string Index = "";
            if (outp.ToUpper() == "L")
            {
                Index = "1";
            }
            else if (outp.ToUpper() == "R")
            {
                Index = "2";
            }
            else
            {
                Index = "1";
            }
            tb_OutpItem = await cls.cls_service_api.Request_Getorderitem(Index);
            showOutputDetait(tb_OutpItem);
        }

        private async void showOutputDetait(DataTable tb)
        {
            DataTable tb2;
            DataTable tbResult;
            string Output = "";
            string result = "";
            List<orderDetail> detail1 = new List<orderDetail>();
            List<orderDetail> detail2 = new List<orderDetail>();
            //try {
            if (tb.Rows.Count > 0 )
            {
                tbResult = tb.AsEnumerable()
                    .GroupBy(r => new { Col1 = r["orderNo"] })
                    .Select(g => g.OrderBy(r => r["orderNo"]).First())
                    .CopyToDataTable();
                string orderNo = "";
                foreach (DataRow i in tbResult.Rows)
                {
                    
                    result = "[\"" + string.Join("\",\"", i["orderNo"].ToString()) + "\"]";
                    orderNo += string.Format("'{0}',", i["orderNo"].ToString());
                }
                orderNo = orderNo.Remove(orderNo.Length - 1, 1);

                //tb2 = md_med.get_OrderInfo(orderNo);
                tb2 = await cls.cls_service_api.Request_Getorderdetails(result);
                if(tb2.Rows.Count > 0)
                {
                    foreach (DataRow r2 in tb2.Rows)
                    {
                        foreach (DataRow r1 in tb.Rows)
                        {
                            if (r2["orderNo"].ToString().Trim() == r1["orderNo"].ToString().Trim())
                            {
                                r1["hn"] = (r2["hn"].ToString() != "") ? r2["hn"].ToString() : r1["hn"].ToString();
                                r1["qn"] = (r2["qn"].ToString() != "") ? r2["qn"].ToString() : r1["qn"].ToString();
                                r1["name"] = (r2["name"].ToString() != "") ? r2["name"].ToString() : r1["name"].ToString();
                                r1["age"] = (r2["age"].ToString() != "") ? r2["age"].ToString() : r1["age"].ToString();
                                r1["gender"] = (r2["gender"].ToString() != "") ? r2["gender"].ToString() : r1["gender"].ToString();
                            }
                        }
                    }

                }

                orderDetail d;
                bool isTwo = false;
                if(tb_OutpItem.Rows.Count > 0)
                {
                    foreach (DataRow r in tb_OutpItem.Rows)
                    {
                        d = new orderDetail();
                        d.no = r["orderNo"].ToString();
                        d.qn = r["qn"].ToString();
                        d.name = r["name"].ToString();
                        d.hn = r["hn"].ToString();
                        d.gender = r["gender"].ToString();
                        d.age = r["age"].ToString();
                        d.drugcode = r["drugcode"].ToString();
                        d.drugname = r["drugname"].ToString();
                        d.qty = r["qty"].ToString();
                        d.unit = r["unit"].ToString();

                        Output = r["outp"].ToString();

                        if (Output == "1" || Output == "3")
                        {
                            detail1.Add(d);
                        }
                        else
                        {
                            detail2.Add(d);
                        }

                    }
                }
                
            }
            ucFrm_Outp1.updateDetail(detail1);
            ucFrm_Outp2.updateDetail(detail2);
        }

        //public static String ProductVersion
        //{
        //    get
        //    {
        //        string location = Assembly.GetCallingAssembly().Location;
        //        DateTime modification = File.GetLastWriteTime(location);
        //        return new Version(FileVersionInfo.GetVersionInfo(location).ProductVersion).ToString() + " Build:" + modification;
        //    }
        //}

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            //MessageBox.Show("Enter");
        }


        public async Task PrintSlipAsync(string prescriptionno)
        {
            //itemidentify = txt_BarcodePrescr.Text;
            //itemidentify = "M67021300001";
            string outpNo = "";
            string itemidentify = "";

            DataTable dt_slip = new DataTable();
            dt_slip.Columns.Add("prescriptionno", typeof(String));
            dt_slip.Columns.Add("prescription_om", typeof(String));
            dt_slip.Columns.Add("orderitembarcode", typeof(String));
            dt_slip.Columns.Add("patientname", typeof(String));
            dt_slip.Columns.Add("hn", typeof(String));
            dt_slip.Columns.Add("an", typeof(String));
            dt_slip.Columns.Add("patientdob", typeof(String));
            dt_slip.Columns.Add("wardname", typeof(String));
            dt_slip.Columns.Add("bedcode", typeof(String));
            dt_slip.Columns.Add("takedate", typeof(String));
            dt_slip.Columns.Add("orderitemname", typeof(String));
            dt_slip.Columns.Add("genericname", typeof(String));
            dt_slip.Columns.Add("Qrcode", typeof(byte[]));
            dt_slip.Columns.Add("orderqty", typeof(String));
            dt_slip.Columns.Add("dosage", typeof(String));
            dt_slip.Columns.Add("orderunitdesc", typeof(String));
            dt_slip.Columns.Add("orderdate", typeof(String));
            dt_slip.Columns.Add("orderitemTHname", typeof(String));
            dt_slip.Columns.Add("locationname", typeof(String));
            dt_slip.Columns.Add("itemidentify", typeof(String));
            dt_slip.Columns.Add("patientGender", typeof(String));
            dt_slip.Columns.Add("patientAge", typeof(String));
            dt_slip.Columns.Add("OutpNo", typeof(String));
            dt_slip.Columns.Add("BarcodeHn", typeof(String));
            dt_slip.Columns.Add("rcvmedno", typeof(String));
            dt_slip.Columns.Add("expressmed", typeof(String));
            dt_slip.Columns.Add("amount", typeof(String));
            dt_slip.Columns.Add("note", typeof(String));
            dt_slip.Columns.Add("qn", typeof(String));
            dt_slip.Columns.Add("basketno", typeof(String));
            dt_slip.Columns.Add("confirm_allergy", typeof(String));

            dt_slip.TableName = "dt_slip";


            DataTable dtorder = new DataTable();
            //dt_slip.WriteXmlSchema("dt_slip.xsd");

            //dtorder = md_med.GetOutporder(prescriptionno);
            dtorder = cls.orderDetail.db_packagemaster.AsEnumerable().Where(r => r.Field<string>("shelfzone") == "SE-MED") // เงื่อนไขเฉพาะ SE-MED
                        .GroupBy(r => new
                        {
                            ShelfZone = r.Field<string>("shelfzone"),
                            OrderItemCode = r.Field<string>("orderitemcode")
                        })
                        .Select(g => g.OrderBy(r => r["orderitemcode"]).First())
                        .CopyToDataTable();
            DataTable dtprescription = new DataTable();
            DataTable dtdrug = new DataTable();
            string[] drugcode;

            if (dtorder.Rows.Count > 0)
            {
                drugcode = new string[dtorder.Rows.Count];
                for (int r = 0; r < dtorder.Rows.Count; r++)
                {
                    drugcode[r] = dtorder.Rows[r]["orderitemcode"].ToString();
                }

                string arraydrug = "[\"" + string.Join("\",\"", drugcode) + "\"]";

                DataTable dt_stock = new DataTable();
                dt_stock = await cls.cls_service_api.Request_Getsemedstock(arraydrug);

               
                itemidentify = dtorder.Rows[0]["prescriptionno_sup"].ToString();
                //outpNo = md_se.get_outp(itemidentify);

                dtprescription = await cls.cls_service_api.Request_getoutput(itemidentify);
                if (dtprescription != null && dtprescription.Rows.Count > 0)
                {
                    outpNo = dtprescription.Rows[0].Table.Columns.Contains("outp")
                             ? dtprescription.Rows[0]["outp"].ToString()
                             : "-";
                }
                else
                {
                    outpNo = "-";
                }

                //ut.log("Get Output No. md_se.get_outp("+itemidentify+")="+ outpNo);
                //System.Threading.Thread.Sleep(2000);

                for (int i = 0; i < dtorder.Rows.Count; i++)
                {

                    //DataTable dtStockDIH = new DataTable();
                    //dtStockDIH = clsDrugDIH.GetDrugLocation_DIH(" WHERE IFNULL(dd.printName, CONCAT(dv.deviceName, '-', ds.positionID)) = '" + dtorder.Rows[i]["takeNote"].ToString() + "' and d.drugCode = '" + dtorder.Rows[i]["drugCode"].ToString() + "'");


                    DataRow r = dt_slip.Rows.Add();
                    r["prescriptionno"] = dtorder.Rows[i]["prescriptionno"].ToString();
                    //r["prescription_om"] = dtorder.Rows[i]["prescription_om"].ToString();
                    r["orderitembarcode"] = ""; // dtorder.Rows[i]["orderitembarcode"].ToString();
                    r["patientname"] = dtorder.Rows[i]["patientname"].ToString();
                    r["hn"] = dtorder.Rows[i]["hn"].ToString();
                    r["an"] = dtorder.Rows[i]["vn"].ToString();
                    r["wardname"] = ""; // dtorder.Rows[i]["wardname"].ToString();
                    r["bedcode"] = dtorder.Rows[i]["basketno"].ToString();
                    r["takedate"] = ""; // dtorder.Rows[i]["takedate"].ToString();
                    r["orderitemname"] = dtorder.Rows[i]["orderitemname"].ToString();
                    r["genericname"] = "";//dtPres.Rows[i]["genericname"].ToString();
                    r["basketno"] = dtorder.Rows[i]["basketno"].ToString();

                    //r["amount"] = dtStockDIH.Rows[0]["amount"].ToString();
                    //if (dtorder.Rows[i]["confirm_allergy"].ToString() != "")
                    //{
                    //    r["confirm_allergy"] = "** ซักแพ้ยา ";
                    //}
                    //else
                    //{
                    //    r["confirm_allergy"] = "";
                    //}

                    r["confirm_allergy"] = "";

                    //string genTXT = "";
                    //genTXT = dtorder.Rows[i]["itemidentify"].ToString(); //itemidentify.ToString();

                    MemoryStream ms = new MemoryStream();
                    ut.genQr(itemidentify).Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] bytes = ms.ToArray();
                    r["Qrcode"] = bytes;

                    //r["rcvmedno"] = dtorder.Rows[i]["rcvmedno"].ToString();
                    r["qn"] = dtorder.Rows[i]["vn"].ToString();
                    if (dt_stock.Rows.Count > 0)
                    {
                        foreach(DataRow rw in dt_stock.Rows)
                        {
                            if(rw["packageRatio"].ToString() != "" && rw["drugCode"].ToString() == dtorder.Rows[i]["orderitemcode"].ToString())
                            {
                                r["orderqty"] = Convert.ToInt32(dtorder.Rows[i]["orderqty"].ToString()) / Convert.ToInt32(rw["packageRatio"].ToString());
                            }
                            else
                            {
                                r["orderqty"] = dtorder.Rows[i]["orderqty"].ToString();
                            }
                        }
                       
                    }
                    else
                    {
                        r["orderqty"] = dtorder.Rows[i]["orderqty"].ToString();
                    }


                    r["dosage"] = ""; // dtorder.Rows[i]["dosage"].ToString();
                    r["orderunitdesc"] = "Box"; // dtorder.Rows[i]["dosage"].ToString();
                    r["orderdate"] = dtorder.Rows[i]["ordercreatedate"].ToString();
                    r["orderitemTHname"] = ""; // dtorder.Rows[i][""].ToString();
                    r["locationname"] = dtorder.Rows[i]["shelfzone"].ToString();
                    r["itemidentify"] = itemidentify; // dtorder.Rows[i]["orderNo"].ToString();
                    r["patientGender"] = ""; /*dtorder.Rows[i]["patientGender"].ToString();*/
                    //dtStockDIH.Rows[0]["amount"] = (dtStockDIH.Rows[0]["amount"].ToString().Length > 0) ? dtStockDIH.Rows[0]["amount"] : "0";
                    dtorder.Rows[i]["orderqty"] = (dtorder.Rows[i]["orderqty"].ToString().Length > 0) ? dtorder.Rows[i]["orderqty"] : "0";

                    //if (Convert.ToDouble(dtorder.Rows[i]["amount"].ToString()) > Convert.ToDouble(dtStockDIH.Rows[0]["amount"].ToString()))
                    //{
                    //    double total = Convert.ToDouble(dtorder.Rows[i]["amount"].ToString()) - Convert.ToDouble(dtStockDIH.Rows[0]["amount"].ToString());

                    //    r["note"] = " ** ยาไม่พอจ่าย   จำนวน " + total.ToString();
                    //}

                    //if (dtorder.Rows[i]["expressmed"].ToString() == "1")
                    //{
                    //    r["expressmed"] = " ยาด่วน ";
                    //}
                    //else
                    //{
                    //    r["expressmed"] = "";
                    //}
                    r["expressmed"] = "";

                    if (dtorder.Rows[i]["patientdob"].ToString() != "")
                    {
                        string yy = convertdate_YYYY_EN(dtorder.Rows[0]["patientdob"].ToString());
                        string mm = convertdate_MM_EN(dtorder.Rows[0]["patientdob"].ToString());
                        string dd = convertdate_DD_EN(dtorder.Rows[0]["patientdob"].ToString());

                        DateTime birthDate = new DateTime(Convert.ToInt16(yy), Convert.ToInt16(mm), Convert.ToInt16(dd));

                        r["patientAge"] = CalculateAge(birthDate);

                    }
                    else { r["patientAge"] = "-"; }


                    //r["patientAge"] = dtorder.Rows[i]["patientAge"].ToString();
                    //r["outpNo"] = dtorder.Rows[i]["outNo"].ToString();
                    r["outpNo"] = outpNo;
                    r["BarcodeHn"] = ""; /*dtorder.Rows[i]["patientID"].ToString();*/

                }
            }


            dtprescription = await cls.cls_service_api.Request_getoutput(itemidentify);
            if (dtprescription != null && dtprescription.Rows.Count > 0)
            {
                outpNo = dtprescription.Rows[0].Table.Columns.Contains("outp")
                         ? dtprescription.Rows[0]["outp"].ToString()
                         : "-";
            }
            else
            {
                outpNo = "-";
            }


            if (dt_slip.Rows.Count > 0 )
            {
                ////ReportDocument rpt = new ReportDocument();
                //CrystalDecisions.CrystalReports.Engine.ReportDocument rpt = new CrystalDecisions.CrystalReports.Engine.ReportDocument();

                //rpt.Load(Application.StartupPath + "\\Report\\crpguideslip.rpt");
                //rpt.SetDataSource(dt_slip);
                //rpt.PrintOptions.PrinterName = Properties.Settings.Default.printguideslip;
                //rpt.PrintToPrinter(0, false, 0, 0);
                if (crpguideslip1 != null) crpguideslip1.DataDefinition.FormulaFields["output_no"].Text = string.Format("'{0}'", outpNo);
                if (crpguideslip1 != null) crpguideslip1.SetDataSource(dt_slip);
                if (crpguideslip1 != null) crpguideslip1.PrintOptions.PrinterName = Properties.Settings.Default.printguideslip;
                if (crpguideslip1 != null) crpguideslip1.PrintToPrinter(0, false, 0, 0);

                dataGridView1.Rows.Clear();
                //ut.log("Print success.");
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
        public string convertdate_MM_EN(string val)
        {
            int buddhistYear = 0;
            var dt = Convert.ToDateTime(val).ToString("yyyy-MM-dd");
            string val_ = dt.ToString();
            string DT = "";
            if (val_ != "")
            {
                DT += val_.Substring(5, 2);

            }
            return DT;
        }
        public string convertdate_DD_EN(string val)
        {
            int buddhistYear = 0;
            var dt = Convert.ToDateTime(val).ToString("yyyy-MM-dd");
            string val_ = dt.ToString();
            string DT = "";
            if (val_ != "")
            {
                DT += val_.Substring(8, 2);

            }
            return DT;
        }

        public static string CalculateAge(DateTime birthDate, DateTime? referenceDate = null)
        {
            DateTime today = referenceDate ?? DateTime.Today;
            string age = "";
            int years = today.Year - birthDate.Year;
            int months = today.Month - birthDate.Month;
            int days = today.Day - birthDate.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(today.Year, (today.Month == 1) ? 12 : today.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }
            age = " " + years + "Y " + months + "M " + days + "D ";


            return age;
        }


        private void crpguideslip1_InitReport(object sender, EventArgs e)
        {

        }

        private void tmtConn_Tick(object sender, EventArgs e)
        {
            if (PingHost(Properties.Settings.Default.DB_SE_SERVER))
            {
                //pnlConnect.BackgroundImage = Properties.Resources.LED_ON_180x180;
            }
            else
            {
                //pnlConnect.BackgroundImage = Properties.Resources.LED_OFF_180x180;
            }
        }

        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }

                                        private void WebView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                webView21.CoreWebView2.SetVirtualHostNameToFolderMapping("app.local", "wwwroot", Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow); 
                webView21.CoreWebView2.Navigate("http://app.local/index.html");
                
                webView21.CoreWebView2.NavigationCompleted += async (s, args) => {
                    string currentLR = Properties.Settings.Default.OUTPUT_LR;
                    if (string.IsNullOrEmpty(currentLR)) currentLR = "L";
                    string script = $@"
                        setInterval(function() {{
                            if (!document.getElementById('gearBtn')) {{
                                let gear = document.createElement('div');
                                gear.id = 'gearBtn';
                                gear.innerHTML = '⚙️';
                                gear.style.cssText = 'position:fixed; bottom:20px; right:20px; z-index:2147483647; font-size:26px; cursor:pointer; background:#fff; border-radius:50%; box-shadow:0 4px 10px rgba(0,0,0,0.3); border: 2px solid #0ea5e9; text-align:center; width:50px; height:50px; line-height:46px; user-select:none; transition: transform 0.2s;';
                                gear.onmouseover = function() {{ gear.style.transform = 'scale(1.1)'; }};
                                gear.onmouseout = function() {{ gear.style.transform = 'scale(1)'; }};
                                
                                let panel = document.createElement('div');
                                panel.id = 'gearPanel';
                                panel.style.cssText = 'display:none; position:fixed; bottom:80px; right:20px; z-index:2147483647; background:#fff; padding:15px; border-radius:8px; box-shadow:0 4px 15px rgba(0,0,0,0.4); border:2px solid #0ea5e9;';
                                panel.innerHTML = `<div style='margin-bottom:8px; font-size:14px; color:#64748b; font-weight:bold;'>ตั้งค่าช่องจ่ายยา</div><select id='sideSelect' style='padding:8px 15px; border-radius:6px; font-size:16px; font-weight:bold; border: 1px solid #cbd5e1; cursor:pointer; outline:none; background:#f8fafc; color:#0f172a;'>
                                    <option value='L'>🖥️ เครื่องฝั่งซ้าย (ช่อง 1, 2)</option>
                                    <option value='R'>🖥️ เครื่องฝั่งขวา (ช่อง 3, 4)</option>
                                </select>`;
                                
                                document.body.appendChild(gear);
                                document.body.appendChild(panel);
                                
                                document.getElementById('sideSelect').value = '{currentLR}';
                                document.getElementById('sideSelect').addEventListener('change', function(evt) {{
                                    window.chrome.webview.postMessage({{ type: 'CHANGE_SIDE', value: evt.target.value }});
                                    panel.style.display = 'none';
                                }});
                                
                                gear.onclick = function() {{
                                    panel.style.display = panel.style.display === 'none' ? 'block' : 'none';
                                }};
                            }}
                        }}, 1000);
                    ";
                    await webView21.CoreWebView2.ExecuteScriptAsync(script);
                };
            }
        }
private async void WebView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson;
                
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                                if (data != null && data.type == "CHANGE_SIDE")
                {
                    string newSide = data.value;
                    Properties.Settings.Default.OUTPUT_LR = newSide;
                    Properties.Settings.Default.Save();
                    MessageBox.Show("เปลี่ยนเป็นเครื่องฝั่ง " + (newSide == "L" ? "ซ้าย (ช่อง 1, 2)" : "ขวา (ช่อง 3, 4)") + " เรียบร้อยแล้ว!\nระบบจะจดจำการตั้งค่านี้ไว้ตลอด", "ตั้งค่าสำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                if (data != null && data.type == "DISPENSE_SOAP")
                {
                    string reqId = data.reqId;
                    dynamic payloadStr = data.xml; 
                    
                    try
                    {
                        dynamic payload = Newtonsoft.Json.JsonConvert.DeserializeObject(payloadStr.ToString());
                        
                        string outp = Properties.Settings.Default.OUTPUT_LR;
                        string windowNoStr = "2";
                        if (outp != null && outp.ToUpper() == "L") { windowNoStr = "1"; }

                        var culture = new System.Globalization.CultureInfo("en-US");

                        List<gd4lib.OPD> opdList = new List<gd4lib.OPD>();
                        if (payload.drugsList != null)
                        {
                            foreach (var drug in payload.drugsList)
                            {
                                gd4lib.OPD d = new gd4lib.OPD();
                                d.patID = payload.patientInfo.hn != null ? payload.patientInfo.hn.ToString() : "";
                                d.patName = payload.patientInfo.patientname != null ? payload.patientInfo.patientname.ToString().Replace("/", "").Replace("'", "") : "";
                                d.gender = payload.patientInfo.sex != null ? payload.patientInfo.sex.ToString() : "";
                                
                                string dobStr = payload.patientInfo.patientdob != null ? payload.patientInfo.patientdob.ToString() : "";
                                if (!string.IsNullOrEmpty(dobStr)) {
                                    DateTime dt;
                                    if (DateTime.TryParse(dobStr, out dt)) {
                                        d.birthday = dt.ToString("yyyy-MM-dd HH:mm:ss", culture);
                                    } else {
                                        d.birthday = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", culture);
                                    }
                                } else {
                                    d.birthday = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", culture);
                                }
                                
                                d.age = payload.patientInfo.age != null ? payload.patientInfo.age.ToString() : "";

                                d.QN = payload.patientInfo.qn != null ? payload.patientInfo.qn.ToString() : "";
                                d.AN = d.patID;
                                
                                d.identity = "";
                                d.insuranceNo = "";
                                d.chargeType = "";
                                d.orderNo = payload.patientInfo.prescriptionno_sup != null ? payload.patientInfo.prescriptionno_sup.ToString() : (payload.patientInfo.prescriptionno != null ? payload.patientInfo.prescriptionno.ToString() : "");
                                d.orderType = "";
                                d.pharmacy = "OPD";
                                d.windowNo = windowNoStr; 
                                d.paymentIP = "";
                                
                                string orderdtStr = payload.patientInfo.ordercreatedate != null ? payload.patientInfo.ordercreatedate.ToString() : "";
                                if (!string.IsNullOrEmpty(orderdtStr)) {
                                    DateTime dt2;
                                    if (DateTime.TryParse(orderdtStr, out dt2)) {
                                        d.paymentDT = dt2.ToString("yyyy-MM-dd HH:mm:ss", culture);
                                    } else {
                                        d.paymentDT = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", culture);
                                    }
                                } else {
                                    d.paymentDT = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", culture);
                                }

                                d.outpNo = "";
                                d.visitNo = payload.patientInfo.vn != null ? payload.patientInfo.vn.ToString() : "";
                                d.deptCode = payload.patientInfo.wardcode != null ? payload.patientInfo.wardcode.ToString() : "";
                                d.deptName = payload.patientInfo.wardname != null ? payload.patientInfo.wardname.ToString() : "";
                                d.doctCode = payload.patientInfo.doctorcode != null ? payload.patientInfo.doctorcode.ToString() : "";
                                d.doctName = payload.patientInfo.doctorname != null ? payload.patientInfo.doctorname.ToString() : "";
                                d.diagnosis = "";
                                d.alias = "";
                                d.code = drug.orderitemcode != null ? drug.orderitemcode.ToString() : "";
                                d.name = drug.orderitemname != null ? drug.orderitemname.ToString() : "";
                                d.spec = drug.Strength != null ? drug.Strength.ToString() : "";
                                d.firmName = drug.firmname != null ? drug.firmname.ToString() : "";
                                d.qty = drug.orderqty != null ? drug.orderqty.ToString() : "";
                                d.unit = drug.orderunitcode != null ? drug.orderunitcode.ToString() : "";
                                d.method = "";
                                d.type = "";
                                d.note = "";
                                d.itemNo = "";
                                
                                opdList.Add(d);
                            }
                        }
                        
                        dih dih_local = new dih();
                        string XML2DIH_OPD = dih_local.genXML2_OPD(opdList);

                        using (dih_webserv.DIHPMPFWebservice dihweb = new dih_webserv.DIHPMPFWebservice())
                        {
                            dihweb.Proxy = null;
                            System.Net.ServicePointManager.Expect100Continue = false;

                            string dihapi_result = dihweb.outpOrderDispense(XML2DIH_OPD);

                            var responseObj = new { type = "SOAP_RESPONSE", reqId = reqId, result = dihapi_result };
                            string responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(responseObj);
                            webView21.CoreWebView2.PostWebMessageAsJson(responseJson);
                        }
                    }
                    catch (Exception ex)
                    {
                        string errMsg = ex.Message;
                        if (ex.InnerException != null) { errMsg += " (" + ex.InnerException.Message + ")"; }
                        var errObj = new { type = "SOAP_RESPONSE", reqId = reqId, error = "C# SOAP ERROR: " + errMsg };
                        string errJson = Newtonsoft.Json.JsonConvert.SerializeObject(errObj);
                        webView21.CoreWebView2.PostWebMessageAsJson(errJson);
                    }
                }
            }
            catch (Exception exOuter)
            {
                // ignore
            }
        }

        private void frm_main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                Application.Exit();
            }
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }

    //Class Other
    [XmlRoot(ElementName = "status")]
    public class Status
    {

        [XmlElement(ElementName = "code")]
        public int Code { get; set; }

        [XmlElement(ElementName = "message")]
        public object Message { get; set; }
    }

    [XmlRoot(ElementName = "result")]
    public class Result
    {

        [XmlElement(ElementName = "status")]
        public Status Status { get; set; }
    }
}

        















