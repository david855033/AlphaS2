using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace AlphaS2
{
    class Sql : IDisposable
    {
        SqlConnection connection;
        const string connectionStr = @"Server=localhost\SQLEXPRESS;database=alphas2;Trusted_Connection=true;";
        public Sql() {
            CreateConnection();
        }
        private void CreateConnection() {
            connection = new SqlConnection(connectionStr);
            connection.Open();
        }
        public void Dispose() {
            connection.Close();
        }

        public void CreateTable(string tableName, IEnumerable<SqlField> field) {
            string commandStr = $"If not exists (select name from sysobjects where name = '{tableName}') CREATE TABLE {tableName}+(" +
                $"{String.Join(",",field.Select(x=>x.GetTableCmdStr()))}";
            Console.WriteLine(commandStr);
            //SqlCommand myCommand = new SqlCommand(commandStr, connection);
            //myCommand.ExecuteNonQuery();
        }
    }
    class SqlField
    {
        public string name="defaultName";
        public string dataType="smallint";
        public bool isNull=false;
        public SqlField(string name, string dataType, bool isNull) {
            this.name = name;this.dataType = dataType;this.isNull = isNull;
        }
        public string GetTableCmdStr() {
            return name + " " + dataType + " " + (isNull ? "NULL" : "NOT NULL");
        }
    }
}
