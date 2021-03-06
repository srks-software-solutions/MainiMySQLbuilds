using System;
using System.Configuration;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace SRKSDAQFanuc
{
    class MSqlConnection : IDisposable
    {
        //static String ServerName = @"PLM"; //@"SVCCSOEE\CCSOEESQLEXPRESS";//@"DESKTOP-NFACHKF\SQL2012EXP013";//
        //static String username = "srkssa";
        //static String password = "srks4$maini";
        //static String port = "3306";
        //static String DB = "unitworksccs";

        public static String ServerName = @"" + ConfigurationManager.AppSettings["ServerName"]; //SIEMENS\SQLEXPRESS
        public static String username = ConfigurationManager.AppSettings["username"]; //sa
                                                                                      //static String password = "srks4$";//server
        public static String password = ConfigurationManager.AppSettings["password"];
        public static String port = "3306";
        public static String DB = ConfigurationManager.AppSettings["DB"];// i_facility_tsal //Common
        public static String Schema = ConfigurationManager.AppSettings["Schema"];  //Schema Name
        public static String DbName = ConfigurationManager.AppSettings["databasename"];

        //  public MySqlConnection msqlConnection = new MySqlConnection("server = " + ServerName + ";userid = " + username + ";Password = " + password + ";database = " + DB + ";port = " + port + ";persist security info=False");

        public MySqlConnection MqlConnection = new MySqlConnection(@"Data Source = " + ServerName + ";User ID = " + username + ";Password = " + password + ";Initial Catalog = " + DB + ";Persist Security Info=True");

        public void open()
        {
            if (MqlConnection.State != System.Data.ConnectionState.Open)
                MqlConnection.Open();
        }

        public void close()
        {
            MqlConnection.Close();
        }

        public void Dispose()
        {
            MqlConnection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
