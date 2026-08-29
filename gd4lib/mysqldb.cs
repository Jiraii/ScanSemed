using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace gd4lib
{
    public class mysqldb
    {
        string ConnectionString = "";
        string SERVER_IP = "";
        string SERVER_USER = "";
        string SERVER_PASSWD = "";
        string SERVER_DBNAME = "";
        string SERVER_PORT = "3306";
        string SERVER_ENCODE = "tis620";

        //string DATABASE_TYPE = "mysql";

        MySqlConnection conn;
        MySqlCommand cmd;
        MySqlDataAdapter adt;

        public mysqldb(string SERVER, string DBNAME, string USER, string PASSWD, string PORT = "3306", string ENCODE = "utf8") //tis620
        {
            SERVER_IP = SERVER;
            SERVER_USER = USER;
            SERVER_PASSWD = PASSWD;
            SERVER_DBNAME = DBNAME;
            SERVER_PORT = PORT;
            SERVER_ENCODE = ENCODE;

            this.mysql_initDB();
        }

        private void mysql_initDB()
        {
            try
            {
                ConnectionString = "server=" + SERVER_IP + ";user id=" + SERVER_USER + @";password=" + SERVER_PASSWD + ";persistsecurityinfo=True;database=" + SERVER_DBNAME + ";port=" + SERVER_PORT + ";maxpoolsize=400;minpoolsize=0;charset=" + SERVER_ENCODE + ";";
                this.conn = new MySqlConnection(ConnectionString);
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
                cmd = new MySqlCommand(sqlQuery, this.conn);
                            
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
                this.cmd = new MySqlCommand(sqlQuery, this.conn);
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
                cmd = new MySqlCommand(sqlQuery, this.conn);
                adt = new MySqlDataAdapter();

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