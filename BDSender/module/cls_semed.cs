using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gd4lib;
using System.Data;
using System.Reflection;

namespace BDSender.module
{
    class cls_semed
    {
        gd4lib.mssqldb db;
        string _SERVER = Properties.Settings.Default.DB_SE_SERVER;
        string _USER = Properties.Settings.Default.DB_SE_USER;
        string _PASS = Properties.Settings.Default.DB_SE_PASS;
        string _NAME = Properties.Settings.Default.DB_SE_NAME;
        string _PORT = Properties.Settings.Default.DB_SE_PORT;

        public cls_semed()
        {
            db = new mssqldb(_SERVER, _NAME, _USER, _PASS, _PORT);
        }

        public DataTable get_order(string OUTPUT_LR)
        {
            string Index = "";
            if (OUTPUT_LR=="L")
            {
                Index = "1,2";
            }else if (OUTPUT_LR == "R")
            {
                Index = "3,4";
            }else
            {
                Index = "1,2";
            }
            DataTable dt;
            string sql = @"SELECT
                            p.createtime as 'createtime',
                            p.orderNo as 'orderid',
                            c.Name as 'patient',
                            p.outp as 'output'
                            FROM vOutpStatus p
                            LEFT JOIN Customers  c on p.CustomerId = c.Id
                            WHERE p.status = 'process' and p.outp in ({0})
                            ORDER BY p.createtime ASC;";

            sql = string.Format(sql, Index);

            dt = db.selectCMD(sql).Tables[0];
            return dt;
        }

        public DataTable get_OrderItem(string OUTPUT_LR)
        {
            string Index = "";
            if (OUTPUT_LR == "L")
            {
                Index = "1,2";
            }
            else if (OUTPUT_LR == "R")
            {
                Index = "3,4";
            }
            else
            {
                Index = "1,2";
            }
            DataTable dt;
            string sql = @"SELECT 
                            lo.outp,
                            lo.orderNo,
                            c.Name as name,
                            '' as hn,
                            '' as qn,
                            'O' as gender,
                            (YEAR(GETDATE()) - YEAR(c.Birthday)) as age,
                            p.Code as drugcode,
                            p.Name as drugname,
                            d.Quantity as qty,
                            p.Unit as Unit,
                            DATEDIFF(Minute, lo.lastcreate, GETDATE()) AS waittime
                             FROM(
	                            SELECT 
	                            s.outp
	                            ,MAX(s.createtime) as lastcreate
	                            ,(SELECT o.OrderId FROM vOutpStatus o WHERE o.createtime=MAX(s.createtime)) as OrderId
	                            ,(SELECT o.orderNo FROM vOutpStatus o WHERE o.createtime=MAX(s.createtime)) as orderNo
	                            ,(SELECT o.CustomerId FROM vOutpStatus o WHERE o.createtime=MAX(s.createtime)) as CustomerLd
	                            FROM vOutpStatus s
	                            where s.outp in ({0}) and s.status = 'success'
	                            GROUP BY s.outp
                            ) lo
                            INNER JOIN Details d on lo.OrderId = d.OrderId
                            INNER JOIN Products p on d.ProductId=p.Id
                            LEFT JOIN Customers c on lo.CustomerLd=c.Id
                            where DATEDIFF(Minute, lo.lastcreate, GETDATE()) < 2
                            ORDER BY lo.outp asc;";
            sql = string.Format(sql, Index);
            dt = db.selectCMD(sql).Tables[0];

            //dtResult = JoinDataTables(dt, dt2,(row1, row2) => row1.Field<string>("orderNo") == row2.Field<string>("orderNo2"));
            return dt;
        }

        public string get_outp(string orderNo)
        {
            string result = "";
            string sql = @"SELECT 
                            a.[Index] as outp
                            FROM Orders o
                            LEFT JOIN Addresses a on o.AddressId = a.Id
                            where o.Code = '{0}';";

            sql = string.Format(sql, orderNo);
            result = db.selectOneValue(sql);
            return result;
        }

        public DataTable get_semedStock(List<string> drugCode)
        {
            DataTable dt;
            string drugInList = "";
            string sql = "";
            foreach(string item in drugCode)
            {
                drugInList += string.Format("'{0}',", item);
            }
            drugInList = drugInList.Substring(0, drugInList.Length - 1);

            sql = @"SELECT
                    s.drugCode as orderitemcode,
                    s.Name as orderitemname,
                    s.Quantity as total
                    FROM vXmedStock s
                    where s.Machine = 'XMed' and s.drugCode in ({0})";
            sql = string.Format(sql, drugInList);

            dt = db.selectCMD(sql).Tables[0];
            return dt;
        }

        private DataTable JoinDataTables(DataTable t1, DataTable t2, params Func<DataRow, DataRow, bool>[] joinOn)
        {
            DataTable result = new DataTable();
            foreach (DataColumn col in t1.Columns)
            {
                if (result.Columns[col.ColumnName] == null)
                    result.Columns.Add(col.ColumnName, col.DataType);
            }
            foreach (DataColumn col in t2.Columns)
            {
                if (result.Columns[col.ColumnName] == null)
                    result.Columns.Add(col.ColumnName, col.DataType);
            }
            foreach (DataRow row1 in t1.Rows)
            {
                var joinRows = t2.AsEnumerable().Where(row2 =>
                {
                    foreach (var parameter in joinOn)
                    {
                        if (!parameter(row1, row2)) return false;
                    }
                    return true;
                });
                foreach (DataRow fromRow in joinRows)
                {
                    DataRow insertRow = result.NewRow();
                    foreach (DataColumn col1 in t1.Columns)
                    {
                        insertRow[col1.ColumnName] = row1[col1.ColumnName];
                    }
                    foreach (DataColumn col2 in t2.Columns)
                    {
                        insertRow[col2.ColumnName] = fromRow[col2.ColumnName];
                    }
                    result.Rows.Add(insertRow);
                }
            }
            return result;
        }


    }
}
