using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
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
            connection.InfoMessage += Connection_InfoMessage;
        }
        public void Dispose() {
            connection.Close();
        }
        string infoMessage = "";
        void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e) {
            infoMessage = e.Message;
        }

        //如果表單不存在則建立
        public void CreateTable(string table, IEnumerable<SqlColumn> column) {
            try {
                string commandStr = $@"If not exists (select name from sysobjects where name = '{table}')
                    BEGIN CREATE TABLE {table}
                    ({String.Join(",", column.Select(x => x.GetTableCmdStr()))})
                    PRINT 'SUCCESS'
                    END ELSE PRINT 'FAIL'";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.ExecuteNonQuery();
                if (infoMessage == "SUCCESS") {
                    Console.WriteLine($"SQL: create table: {table}, Columns({column.Count()})");
                } else {
                    Console.WriteLine($"SQL: {table} exists");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        //如果表單存在則刪除表單
        public void DropTable(string table) {
            try {
                string commandStr = $@"If exists (select name from sysobjects where name = '{table}')
                    BEGIN DROP TABLE {table}
                    PRINT 'SUCCESS'
                    END ELSE PRINT 'FAIL'";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.ExecuteNonQuery();
                if (infoMessage == "SUCCESS") {
                    Console.WriteLine($"SQL: drop table: {table}");
                } else {
                    Console.WriteLine($"SQL: {table} not exists");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        public void SetPrimaryKey(string table, string column) {
            try {
                string commandStr = $@"IF COL_LENGTH('{table}', '{column}') IS NOT NULL
                    BEGIN ALTER TABLE {table} ADD PRIMARY KEY({column});
                    PRINT 'SUCCESS'
                    END ELSE PRINT 'FAIL'";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.ExecuteNonQuery();
                if (infoMessage == "SUCCESS") {
                    Console.WriteLine($"SQL: Set Primary Key, table: {table}, Column: {column}");
                } else {
                    Console.WriteLine($"SQL: {table}-{column} not exists");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        public void SetConstraintPrimaryKey(string table, string[] columns) {

        }
        public void AddColumn(string table, SqlColumn column) {
            try {
                string commandStr = $@"IF COL_LENGTH('{table}', '{column.name}') IS NULL
                    BEGIN ALTER TABLE {table} ADD {column.GetTableCmdStr()}
                    PRINT 'SUCCESS'
                    END ELSE PRINT 'FAIL'";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.ExecuteNonQuery();
                if (infoMessage == "SUCCESS") {
                    Console.WriteLine($"SQL: Add Column, table: {table}, Column: {column.name}");
                } else {
                    Console.WriteLine($"SQL: {table}-{column.name} exists");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void DropColumn(string table, string column) {
            try {
                string commandStr = $@"IF COL_LENGTH('{table}', '{column}') IS NOT NULL
                    BEGIN ALTER TABLE {table} DROP COLUMN {column}
                    PRINT 'SUCCESS'
                    END ELSE PRINT 'FAIL'";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.ExecuteNonQuery();
                if (infoMessage == "SUCCESS") {
                    Console.WriteLine($"SQL: Drop Column, table: {table}, Column: {column}");
                } else {
                    Console.WriteLine($"SQL: {table}-{column} not exists");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void Insert(string table, SqlInsertData insertData) {
            try {
                string commandStr = insertData.GetInserQuery(table);
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                insertData.AddCmdParameters(sqlCommand);
                var ColumnList = insertData.ColumnList;
                var dataList = insertData.dataList;
                foreach (var row in dataList) {
                    for (int i = 0; i < ColumnList.Count; i++) {
                        sqlCommand.Parameters["@" + ColumnList[i].name].Value = row[i];
                    }
                    int affectedRow = sqlCommand.ExecuteNonQuery();
                    Console.WriteLine($"SQL: Insert, ({affectedRow})");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
       
    }

    class SqlColumn
    {
        public string name = "defaultName";
        public string type = "smallint";
        public SqlDbType sqlDbType;
        public bool isNull = false;
        public SqlColumn(string name, string dataType, bool isNull) {
            this.name = name; this.type = dataType; this.isNull = isNull;
        }
        public SqlColumn(string name, SqlDbType sqlDbType) {
            this.name = name; this.sqlDbType = sqlDbType;
        }
        public string GetTableCmdStr() {
            return name + " " + type + " " + (isNull ? "NULL" : "NOT NULL");
        }
    }

    class SqlInsertData
    {
        public List<SqlColumn> ColumnList = new List<SqlColumn>();
        public List<object[]> dataList = new List<object[]>();
        public void AddColumn(string name, SqlDbType sqlDbType) {
            ColumnList.Add(new SqlColumn(name, sqlDbType));
        }
        public string GetInserQuery(string table) {
            return $@"INSERT INTO {table}({String.Join(",", ColumnList.Select(x => x.name))})
                VALUES({String.Join(",", ColumnList.Select(x => "@" + x.name))})";
        }
        public void AddCmdParameters(SqlCommand sqlCommand) {
            foreach (var Column in ColumnList) {
                sqlCommand.Parameters.Add(new SqlParameter("@" + Column.name, Column.sqlDbType));
            }
        }
        public void AddData(object[] add) {
            dataList.Add(add);
        }
    }
}