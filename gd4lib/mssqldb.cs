using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace gd4lib
{
    public class mssqldb
    {
         string ConnectionString = "";
        string SERVER_IP = "";
        string SERVER_USER = "";
        string SERVER_PASSWD = "";
        string SERVER_DBNAME = "";
        string SERVER_PORT = "3306"; //3306
        //string SERVER_ENCODE = "tis620";

        //string DATABASE_TYPE = "mysql";

        SqlConnection conn;
        SqlCommand cmd;
        SqlDataAdapter adt;

        public mssqldb(string SERVER, string DBNAME, string USER, string PASSWD, string PORT = "1433") //tis620
        {
            SERVER_IP = SERVER;
            SERVER_USER = USER;
            SERVER_PASSWD = PASSWD;
            SERVER_DBNAME = DBNAME;
            SERVER_PORT = PORT;
            //SERVER_ENCODE = ENCODE;

            this.sql_initDB();
        }

        private void sql_initDB()
        {
            try
            {
                ConnectionString = "Data Source={0},{1};Initial Catalog={2};Persist Security Info=True;User ID={3};Password={4}";
                ConnectionString = string.Format(ConnectionString, SERVER_IP, SERVER_PORT, SERVER_DBNAME, SERVER_USER, SERVER_PASSWD);
                this.conn = new SqlConnection(ConnectionString);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

#region SQL Excecute
        public bool executeCMD(string sqlQuery)
        {
            try
            {
                bool status = false;
                connOpen();
                cmd = new SqlCommand(sqlQuery, this.conn);
                            
                if (cmd.ExecuteNonQuery() > 0)
                    status = true;
                connClose();
                return status;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string selectOneValue(string sqlQuery)
        {
            try
            {
                string result = "";
                var dt = new DataTable();
                connOpen();
                this.cmd = new SqlCommand(sqlQuery, this.conn);
                result = this.cmd.ExecuteScalar().ToString();
                connClose();
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public DataSet selectCMD(string sqlQuery)
        {
            try
            {
                DataSet ds = new DataSet();
                connOpen();
                cmd = new SqlCommand(sqlQuery, this.conn);
                adt = new SqlDataAdapter();

                adt.SelectCommand = cmd;
                adt.Fill(ds);
                connClose();
                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                DataSet ds = new DataSet();
                return ds;
            }
        }
#endregion

        public void connOpen()
        {
            try
{
                this.conn.Open();            
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public void connClose()
        {
            try
            {
                this.conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public void connDispose()
        {
            try
            {
                this.conn.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
