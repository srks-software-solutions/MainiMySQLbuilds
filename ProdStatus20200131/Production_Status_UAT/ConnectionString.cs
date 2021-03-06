using System;
//using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace Production_Status_UAT
{
    class ConnectionString:IDisposable
    {
        //local
        //static String ServerName = "SRKS-TECH3-PC\\SQLEXPRESS";
        //static String username = "sa";
        //static String password = "srks4$";
        //static String DB = "i_facility_tsal";


        public static String ServerName = @"" + ConfigurationManager.AppSettings["ServerName"]; //SIEMENS\SQLEXPRESS
        public static String username = ConfigurationManager.AppSettings["username"]; //sa
                                                                                      //static String password = "srks4$";//server
        public static String password = ConfigurationManager.AppSettings["password"];
        public static String port = "3306";
        public static String DB = ConfigurationManager.AppSettings["DB"];// i_facility_tsal //Common
        public static String Schema = ConfigurationManager.AppSettings["Schema"];  //Schema Name

        //server
        //static String ServerName = @"TCP:10.20.10.65,1433";
        //static String username = "sa";
        //static String password = "srks4$tsal";
        //static String DB = "i_facility_tsal";

        public MySqlConnection msqlConnection = new MySqlConnection(@"Data Source = " + ServerName + ";User ID = " + username + ";Password = " + password + ";Initial Catalog = " + DB + ";Persist Security Info=True");
        public void open()
        {
            if (msqlConnection.State != System.Data.ConnectionState.Open)
                msqlConnection.Open();
        }

        public void close()
        {
            msqlConnection.Close();
        }

        public void Dispose()
        {
            msqlConnection.Dispose();
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {


        }
    }
}
