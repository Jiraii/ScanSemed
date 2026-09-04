using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gd4lib;
using System.Data;
using BDSender.cls;

namespace BDSender.module
{
    class cls_meddispense
    {
        gd4lib.mysqldb db;
        string _SERVER = Properties.Settings.Default.DB_MED_SERVER;
        string _USER = Properties.Settings.Default.DB_MED_USER;
        string _PASS = Properties.Settings.Default.DB_MED_PASS;
        string _NAME = Properties.Settings.Default.DB_MED_NAME;
        string _PORT = Properties.Settings.Default.DB_MED_PORT;
        orderDetail cls_orderDetail = new orderDetail();
       
        public cls_meddispense()
        {
            db = new mysqldb(_SERVER, _NAME, _USER, _PASS, _PORT);
        }

        public DataTable get_OrderItem(string txtSearch)
        {
            DataTable dt;
            string sql = @"SELECT
	`d`.`ordercreatedate` AS `ordercreatedate`,
	`d`.`prescriptionno` AS `prescriptionno`,
    `d`.`prescriptionno_sup` AS prescriptionno_sup,
	`d`.`orderitembarcode` AS `orderitembarcode`,
	`d`.`queue` AS `queue`,
	`d`.`hn` AS `hn`,
     d.an as an,
	`d`.`patientname` AS `patientname`,
	`d`.`sex` AS `sex`,
	`d`.`patientdob` AS `patientdob`,
     d.age as age,
	`d`.`wardcode` AS `wardcode`,
	`d`.`wardname` AS `wardname`,
	`d`.`doctorcode` AS `doctorcode`,
	`d`.`doctorname` AS `doctorname`,
	`d`.`orderitemcode` AS `orderitemcode`,
	`d`.`orderitemname` AS `orderitemname`,
	`d`.`orderqty` AS `orderqty`,
    '' as total,
	`d`.`orderunitcode` AS `orderunitcode`,
	`d`.`Strength` AS `Strength`,
	`d`.`firmname` AS `firmname`,
	`d`.`shelfzone` AS `shelfzone`,
	`d`.`shelfname` AS `shelfname`,
	`d`.`JobUserID` AS `JobUserID`,
	`d`.`JobDatetime` AS `JobDatetime`,
	`d`.`basketid` AS `basketid`,
     d.drug_allergy
     FROM
	        v_order_detail d
        WHERE
	        d.shelfzone = 'SE'
        AND d.prescriptionno_sup = (select prescriptionno_sup FROM v_order_detail 
		WHERE JobDatetime = (SELECT MAX(JobDatetime) FROM v_order_detail WHERE basketid = '{0}') and basketid = '{0}' limit 0,1) AND shelfzone = 'SE';";
            sql = string.Format(sql, txtSearch);

            dt = db.selectCMD(sql).Tables[0];
            return dt;
        }
       

        public DataTable get_OrderInfo(string orderNo)
        {
            DataTable dt;
            string sql = @"";

            sql = @"SELECT 
                    t.orderNo as 'orderNo',
                    t.visitNo as 'qn',
                    t.patientName as 'name',
                    t.patientID as 'hn',
                    t.patientAge as 'age',
                    t.patientGender as 'gender'
                    from pmpf_server.outporderinfo t
                    WHERE t.orderNo in ({0});";

            sql = string.Format(sql, orderNo);

            dt = db.selectCMD(sql).Tables[0];
            return dt;
        }

        public DataTable GetOutporder(string prescriptionno)
        {
            DataTable dt;
            string sql = @"SELECT 
								m.patientname as patientName,
								m.hn as patientID,
								m.orderitemcode as drugCode,
								m.orderitemname as drugName,
								m.prescriptionno as orderNo,
								(SELECT MAX(rcvmedno) FROM prescription where prescriptionno_sup =  m.prescriptionno_sup) as rcvmedno,
								f.orderNo as itemidentify,
								m.prescription_om as prescription_om,
								ROUND(m.orderqty, 0) as amount,
								m.ordercreatedate as createdDT,
								m.shelfname as takeNote,
								m.queue as QN,
								(SELECT MAX(expressmed) FROM prescription where prescriptionno_sup =  m.prescriptionno_sup) as expressmed,
								(SELECT MAX(confirm_allergy) FROM prescription where prescriptionno_sup =  m.prescriptionno_sup) as confirm_allergy,
								m.sex as patientGender,
								m.patientdob as patientAge,
								'' as drug_allergy
								FROM packagemaster m
								LEFT JOIN pmpf_server.outporderinfo f on substring_index(f.srcOrderNo, ',' ,-(1))  = m.prescriptionno_sup
								where m.prescriptionno_sup = '{0}' AND m.shelfzone = 'SE';";
            sql = string.Format(sql, prescriptionno);

            dt = db.selectCMD(sql).Tables[0];
            return dt;
        }

        public void setLotExpire()
        {
            string sql = @"UPDATE `packagemaster` SET `itemlotcode`='test1', `itemlotexpire`='2025-04-09 12:00:00' WHERE (`prescriptionno`='6727000080') AND (`prescription_om`='O-M01-1080') ";
        }
    }
}
