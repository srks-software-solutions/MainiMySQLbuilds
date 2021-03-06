using System;
//using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace SRKSDAQFanucToolLife
{
    class MsqlConnection : IDisposable
    {
        static String ServerName = @"localhost";
        static String username = "root";
        static String password = "srks4$maini";
        static String port = "3306";
        static String DB = "unitworksccs";

        //public MySqlConnection msqlConnection = new MySqlConnection("server = " + ServerName + ";userid = " + username + ";Password = " + password + ";database = " + DB + ";port = " + port + ";persist security info=False;SslMode=None");

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
    }
}
