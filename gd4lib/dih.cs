using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gd4lib
{
    public class dih
    {
        public string xmlMessage;
        public string genXML2drugDict(drug data)
        {
            string XML = "";
            drugDict drug = new gd4lib.drugDict();
            drug d = new drug();
            d.code = (data.code != null) ? data.code : "";  //required
            d.name = (data.name != null) ? data.name : "";  //required
            d.miniSpec = (data.miniSpec != null) ? data.miniSpec : "";  //required
            d.miniUnit = (data.miniUnit != null) ? data.miniUnit : "";  //required
            d.miniDose = (data.miniDose != null) ? data.miniDose : "";
            d.doseUnit = (data.doseUnit != null) ? data.doseUnit : ""; 
            d.packageSpec = (data.packageSpec != null) ? data.packageSpec : "";  //required
            d.packageUnit = (data.packageUnit != null) ? data.packageUnit : "";  //required
            d.packageRatio = (data.packageRatio != null) ? data.packageRatio : "";  //required
            d._class = (data._class!=null)? data._class: "";
            d.doseForm = (data.doseForm != null) ? data.doseForm : "";
            d.attribute = (data.attribute != null) ? data.attribute : ""; 
            d.property = (data.property != null) ? data.property : ""; 
            d.refrigerated = (data.refrigerated != null) ? data.refrigerated : ""; 
            d.controlled = (data.controlled != null) ? data.controlled : ""; 
            d.firmName = (data.firmName != null) ? data.firmName : "";  //required
            d.barcode = (data.barcode != null) ? data.barcode : ""; 
            d.supervisionCode = (data.supervisionCode != null) ? data.supervisionCode : "";
            d.PYCode = (data.PYCode != null) ? data.PYCode : ""; //required
            d.enable = (data.enable != null) ? data.enable : ""; //required

            drug.drug = d;
            XML = MySerializer<drugDict>.Serialize(drug);
            XML = XML.Replace("_", "");
            return XML;
        }

        public string genXML2_OPD(List<OPD> data)
        {
            Patient obj_patient = new Patient();
            obj_patient.PatID = data[0].patID.Trim(); 
            obj_patient.PatName = data[0].patName.Trim();
            obj_patient.Gender = data[0].gender.Trim();
            obj_patient.Birthday = Convert.ToDateTime(data[0].birthday.Trim()).ToString("yyyy-MM-dd HH:mm:ss");
            obj_patient.Age = data[0].age.Trim();
            obj_patient.Identity = data[0].identity.Trim();
            obj_patient.InsuranceNo = data[0].insuranceNo.Trim();
            obj_patient.ChargeType = data[0].chargeType.Trim();

            //Drugs obj_Drugs = new Drugs();
            Drug obj_Drug;
            List<Drug> obj_Drug_ls = new List<Drug>();

            for (int i = 0; i < data.Count; i++)
            {
                obj_Drug = new Drug();

                obj_Drug.Alias = data[i].alias.Trim();
                obj_Drug.Code = data[i].code.Trim();
                obj_Drug.Name = data[i].name.Trim();
                obj_Drug.Spec = data[i].spec.Trim();
                obj_Drug.FirmName = data[i].firmName.Trim();
                obj_Drug.Qty = data[i].qty.Trim();
                obj_Drug.Unit = data[i].unit.Trim();
                obj_Drug.Method = data[i].method.Trim();
                obj_Drug.Type = data[i].type.Trim();
                obj_Drug.note = data[i].note.Trim();
                obj_Drug.ItemNo = data[i].itemNo.Trim();

                obj_Drug_ls.Add(obj_Drug);
            }
            //obj_Drugs.Drug = obj_Drug_ls;


            Prescription obj_Prescription = new Prescription();
            obj_Prescription.OrderNo = data[0].orderNo.Trim();
            obj_Prescription.QN = data[0].QN.Trim();
            obj_Prescription.AN = data[0].AN.Trim();
            obj_Prescription.Ordertype = data[0].orderType.Trim();
            obj_Prescription.Pharmacy = data[0].pharmacy.Trim();
            obj_Prescription.WindowNo = data[0].windowNo.Trim();
            obj_Prescription.PaymentIP = data[0].paymentIP.Trim();
            obj_Prescription.PaymentDT = Convert.ToDateTime(data[0].paymentDT.Trim()).ToString("yyyy-MM-dd HH:mm:ss");
            obj_Prescription.OutpNo = data[0].outpNo.Trim();
            obj_Prescription.VisitNo = data[0].visitNo.Trim();
            obj_Prescription.DeptCode = data[0].deptCode.Trim();
            obj_Prescription.DeptName = data[0].deptName.Trim();
            obj_Prescription.DoctCode = data[0].doctCode.Trim();
            obj_Prescription.DoctName = data[0].doctName.Trim();
            obj_Prescription.Diagnosis = data[0].diagnosis.Trim();
            obj_Prescription.Drugs = obj_Drug_ls;

            Prescriptions obj_Prescriptions = new Prescriptions();
            obj_Prescriptions.Prescription = obj_Prescription;

            OutpOrderDispense obj_OutpOrderDispense = new OutpOrderDispense();
            obj_OutpOrderDispense.Patient = obj_patient;
            obj_OutpOrderDispense.Prescriptions = obj_Prescriptions;

            this.xmlMessage = MySerializer<OutpOrderDispense>.Serialize(obj_OutpOrderDispense);
            
            return xmlMessage;
        }
    }

    //class drugDict
    public class drugDict
    {
        public drug drug { get; set; }
    }

    public class drug
    {
        public string code { get; set; }
        public string name { get; set; }
        public string miniSpec { get; set; }
        public string miniUnit { get; set; }
        public string miniDose { get; set; }
        public string doseUnit { get; set; }
        public string packageSpec { get; set; }
        public string packageUnit { get; set; }
        public string packageRatio { get; set; }
        public string _class { get; set; }
        public string doseForm { get; set; }
        public string attribute { get; set; }
        public string property { get; set; }
        public string refrigerated { get; set; }
        public string controlled { get; set; }
        public string firmName { get; set; }
        public string barcode { get; set; }
        public string supervisionCode { get; set; }
        public string PYCode { get; set; }
        public string enable { get; set; }
    }

    //class 
    public class Patient
    {
        public string PatID { get; set; }
        public string PatName { get; set; }
        public string Gender { get; set; }
        public string Birthday { get; set; }
        public string QN { get; set; }
        public string AN { get; set; }
        public string Age { get; set; }
        public string Identity { get; set; }
        public string InsuranceNo { get; set; }
        public string ChargeType { get; set; }
    }
    
    public class Drug
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Spec { get; set; }
        public string FirmName { get; set; }
        public string Unit { get; set; }
        public string Alias { get; set; }
        public string Method { get; set; }
        public string Type { get; set; }        
        public string Qty { get; set; }
        public string note { get; set; }
        public string ItemNo { get; set; }
    }
    
    public class Drugs
    {
        public List<Drug> Drug { get; set; }
    }


    public class Prescription
    {
        public string OrderNo { get; set; }
        public string QN { get; set; }
        public string AN { get; set; }
        public string Ordertype { get; set; }
        public string Pharmacy { get; set; }
        public string WindowNo { get; set; }
        public string PaymentIP { get; set; }
        public string PaymentDT { get; set; }
        public string OutpNo { get; set; }
        public string VisitNo { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string DoctCode { get; set; }
        public string DoctName { get; set; }
        public string Diagnosis { get; set; }
        public List<Drug> Drugs { get; set; }
    }
    
    public class Prescriptions
    {
        public Prescription Prescription { get; set; }
    }
    
    public class OutpOrderDispense
    {
        public Patient Patient { get; set; }
        public Prescriptions Prescriptions { get; set; }
    }

    public class OPD
    {
        public string patID; // 1
        public string patName; // ชื่อ-นามสกุล
        public string gender; // เพศ
        public string birthday; // วันเกิด
        public string QN; //เลขคิว
        public string AN; // เลขการรักษา
        public string age; // อายุ
        public string identity; // 
        public string insuranceNo; // 
        public string chargeType; // 
        public string orderNo;   // เลขที่ Order
        public string orderType; // 
        public string pharmacy; // ห้องจัดยา
        public string windowNo; // เลขที่ช่องจ่าย
        public string paymentIP; // 
        public string paymentDT; // วันที่จ่าย
        public string outpNo; //
        public string visitNo; // เลขที่การรักษา
        public string deptCode; //รหัสแผนก
        public string deptName; // ชื่อแผนก
        public string doctCode; // รหัสหมอ
        public string doctName; // ชื่อหมอ
        public string diagnosis; // 
        public string alias; //
        public string code; // รหัสยาที่จ่าย
        public string name; // ชื่อยาที่จ่าย
        public string spec; // ความแรงยา
        public string firmName; // ผู้ผลิตยา
        public string qty; // จำนวนที่จ่าย
        public string unit; // หน่วยที่จ่าย
        public string method; // จำนวนเม็ดที่ทาน
        public string type; // 
        public string note; // 1 (จ่าย1วัน)
        public string itemNo; // 20220304 วันที่จ่าย
    }

}
