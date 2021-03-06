using MySql.Data.MySqlClient;
using System;
using System.Data.SqlClient;

namespace MimicsUpdation 
{
    class MsqlConnection : IDisposable
    {
        //Server
        //static String ServerName = @"TCP:172.16.8.5,1433";
        //static String username = "sa";
        //static String password = "srks4$maini";
        //static String port = "3306";
        //static String DB = "unitworksccs";

        static String ServerName = @"localhost";
        static String username = "root";
        static String password = "@1234";
        static String port = "3306";
        static String DB = "unitworksccs";

        //Local
        //static String ServerName = @"TCP:DESKTOP-M96NU10\SQLEXPRESS,7015";
        //static String username = "sa";
        //static String password = "srks4$";
        //static String DB = "unitworksccs";

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
