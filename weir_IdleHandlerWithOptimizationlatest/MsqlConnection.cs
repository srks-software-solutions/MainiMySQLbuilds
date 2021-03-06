using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
//using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facility_IdleHandlerWithOptimization
{
    class MsqlConnection:IDisposable
    {
      

        public static String ServerName = @"" + ConfigurationManager.AppSettings["ServerName"]; //SIEMENS\SQLEXPRESS
        public static String username = ConfigurationManager.AppSettings["username"]; //sa
                                                                                      //static String password = "srks4$";//server
        public static String password = ConfigurationManager.AppSettings["password"];
        public static String port = "3306";
        public static String DB = ConfigurationManager.AppSettings["DB"];// i_facility_tsal //Common
        public static String Schema = ConfigurationManager.AppSettings["Schema"];  //Schema Name

       
        public MySqlConnection sqlConnection = new MySqlConnection(@"Data Source = " + ServerName + ";User ID = " + username + ";Password = " + password + ";Initial Catalog = " + DB + ";Persist Security Info=True");

        public void open()
        {
            if (sqlConnection.State != System.Data.ConnectionState.Open)
                sqlConnection.Open();
        }

        public void close()
        {
            sqlConnection.Close();
        }

        public void Dispose()
        {
            sqlConnection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
