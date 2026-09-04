using gd4lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDSender.module
{
    class cls_pmpf
    {
        gd4lib.mysqldb db;
        string _SERVER = "";//Properties.Settings.Default.DB_PMPF_SERVER;
        string _USER = "";//Properties.Settings.Default.DB_PMPF_USER;
        string _PASS = "";//Properties.Settings.Default.DB_PMPF_PASS;
        string _NAME = "";//Properties.Settings.Default.DB_PMPF_NAME;
        string _PORT = "";//Properties.Settings.Default.DB_PMPF_PORT;

        public cls_pmpf()
        {
            db = new mysqldb(_SERVER, _NAME, _USER, _PASS, _PORT);
        }

        public DataTable get_OrderInfo(string orderNo1,string orderNo2)
        {
            DataTable dt;
            string sql = @"SELECT 
                            inf.orderNo as 'orderNo',
                            inf.patientName as 'name',
                            inf.patientID as 'hn',
                            inf.patientAge as 'age',
                            inf.patientGender as 'gender'
                            FROM outporderinfo inf
                            where inf.orderNo in ('{0}','{1}')";
            sql = string.Format(sql, orderNo1,orderNo2);

            dt = db.selectCMD(sql).Tables[0];
            return dt;
        }

    }
}
