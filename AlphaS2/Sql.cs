using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace AlphaS2
{
    class Sql:IDisposable
    {
        SqlConnection connection;
        const string connectionStr = @"Server=localhost;Integrated security=SSPI;database=master";
        public Sql() {
            connection = new SqlConnection(connectionStr);
            connection.Open();
        }

        public void Dispose() {
            connection.Close();
        }
    }
}
