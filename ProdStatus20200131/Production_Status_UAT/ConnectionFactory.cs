using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Production_Status_UAT
{
    public class ConnectionFactory: IConnectionFactory
    {
        //Local
        //static String ServerName = @"SRKS-TECH3-PC\\SQLEXPRESS";
        //static String username = "sa";
        //static String password = "srks4$";
        //static String DB = "i_facility_tsal";

        //server
        //static String ServerName = @"TCP:10.20.10.65,1433";
        //static String username = "sa";
        //static String password = "srks4$tsal";
        //static String DB = "i_facility_tsal";

        public static String ServerName = @"" + ConfigurationManager.AppSettings["ServerName"]; //SIEMENS\SQLEXPRESS
        public static String username = ConfigurationManager.AppSettings["username"]; //sa
                                                                                      //static String password = "srks4$";//server
        public static String password = ConfigurationManager.AppSettings["password"];
        public static String port = "3306";
        public static String DB = ConfigurationManager.AppSettings["DB"];// i_facility_tsal //Common
        public static String Schema = ConfigurationManager.AppSettings["Schema"];  //Schema Name


        public readonly string connectionString = @"Data Source = " + ServerName + "; User ID = " + username + "; Password = " + password + ";Initial Catalog = " + DB + "; Persist Security Info=True";

        public IDbConnection GetConnection
        {

            get
            {
                var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                var conn = factory.CreateConnection();
                conn.ConnectionString = connectionString;
                conn.Open();
                return conn;
            }


        }
    }
}
