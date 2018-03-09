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
        string connectionStr = @"Server=localhost\SQLEXPRESS;database=alphas2;Trusted_Connection=true;";

        public Sql() {
            if (Environment.MachineName == "DAVID2015") {
                connectionStr = @"Server=localhost\SQLEXPRESS01;Database=master;Trusted_Connection=True;";
            }
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
                sqlCommand.CommandTimeout = 0;
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
                sqlCommand.CommandTimeout = 0;
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
        //設定主鍵
        public void SetPrimaryKey(string table, string column) {
            try {
                string commandStr = $@"IF COL_LENGTH('{table}', '{column}') IS NOT NULL
                    BEGIN ALTER TABLE {table} ADD PRIMARY KEY({column});
                    PRINT 'SUCCESS'
                    END ELSE PRINT 'FAIL'";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
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
        //設定複合式主鍵
        public void SetPrimaryKeys(string table, string[] columns) {
            try {
                string commandStr = $@"ALTER TABLE {table}
                    ADD CONSTRAINT PK_{table}_{String.Join("_", columns)} PRIMARY KEY ({String.Join(",", columns)});";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
                sqlCommand.ExecuteNonQuery();
                Console.WriteLine($"SQL: Set Constraint Primary Key, table: {table}, columns: {String.Join(",", columns)}");
            } catch (Exception e) {
                Console.WriteLine($"SQL: Fail to Set Constraint Primary Key, table: {table}, columns: {String.Join(",", columns)}");
                Console.WriteLine(e.ToString());
            }
        }
        //移除主鍵
        public void DropPrimaryKey(string table) {
            try {
                string commandStr = $@"DECLARE @table NVARCHAR(512), @sql NVARCHAR(MAX);
                    SELECT @table = N'dbo.{table}';
                    SELECT @sql = 'ALTER TABLE ' + @table 
                    + ' DROP CONSTRAINT ' + name + ';'
                    FROM sys.key_constraints
                    WHERE [type] = 'PK'
                    AND [parent_object_id] = OBJECT_ID(@table);
                    EXEC sp_executeSQL @sql;";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
                sqlCommand.ExecuteNonQuery();
                Console.WriteLine($"SQL: Drop Primary Key,table: {table}");
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        //設定外來鍵
        public void SetForeignKey(string table, string column, string refTable, string refColumn) {
            try {
                string commandStr = $@"IF COL_LENGTH('{table}', '{column}') IS NOT NULL
                    BEGIN ALTER TABLE {table} ADD FOREIGN KEY({column}) REFERENCES {refTable}({refColumn});
                    PRINT 'SUCCESS'
                    END ELSE PRINT 'FAIL'";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
                sqlCommand.ExecuteNonQuery();
                if (infoMessage == "SUCCESS") {
                    Console.WriteLine($"SQL: Set Foreign Key, table: {table}, Column: {column}");
                } else {
                    Console.WriteLine($"SQL: {table}-{column} not exists");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        //設定外來鍵
        public void SetForeignKeys(string table, string[] columns, string refTable, string[] refColumns) {
            try {
                string commandStr = $@"ALTER TABLE {table} ADD FOREIGN KEY({String.Join(",", columns)}) REFERENCES {refTable}({String.Join(",", refColumns)});";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
                sqlCommand.ExecuteNonQuery();
                if (infoMessage == "SUCCESS") {
                    Console.WriteLine($"SQL: Set Foreign Key, table: {table}, Column: {String.Join(",", columns)}");
                } else {
                    Console.WriteLine($"SQL: {table} not exists");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        //新增欄位
        public void AddColumn(string table, SqlColumn column) {
            try {
                string commandStr = $@"IF COL_LENGTH('{table}', '{column.name}') IS NULL
                    BEGIN ALTER TABLE {table} ADD {column.GetTableCmdStr()}
                    PRINT 'SUCCESS'
                    END ELSE PRINT 'FAIL'";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
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
        //刪除欄位
        public void DropColumn(string table, string column) {
            try {
                string commandStr = $@"IF COL_LENGTH('{table}', '{column}') IS NOT NULL
                    BEGIN ALTER TABLE {table} DROP COLUMN {column}
                    PRINT 'SUCCESS'
                    END ELSE PRINT 'FAIL'";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
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
        //新增資料
        public bool InsertUpdateRow(string table, SqlInsertData insertData) {
            string lastData = "";
            if (insertData.DataList.Count() == 0) { return false; }
            try {
                string commandStr = insertData.GetInsertQuery(table);
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
                insertData.AddCmdParameters(sqlCommand);
                var ColumnList = insertData.ColumnList;
                var dataList = insertData.DataList;
                foreach (var row in dataList) {
                    lastData = "";
                    for (int i = 0; i < ColumnList.Count; i++) {
                        sqlCommand.Parameters["@" + ColumnList[i].name].Value = row[i];
                        sqlCommand.Parameters["@" + ColumnList[i].name].DbType =
                            insertData.ColumnList[i].dbType;
                        lastData += $@"{row[i]}:{insertData.ColumnList[i].dbType}" + "\r\n";
                    }
                    sqlCommand.ExecuteNonQuery();
                }
                //Console.WriteLine($"SQL: Insert, ({table}) {insertData.DataList.Count} rows");
            } catch (Exception e) {
                Console.WriteLine($@"ERROR: {lastData}");
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }
        //新增列
        public Boolean UpdateRow(string table, Dictionary<string, string> setKeyValue, string[] condition) {
            try {
                string commandStr = $@"update {table}
                        set {String.Join(",", setKeyValue.Keys.Select(x => x + "=" + setKeyValue[x]))}
                        where {String.Join(" and ", condition)}";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
                sqlCommand.ExecuteNonQuery();
                //Console.WriteLine($"SQL: Update Row, table: {table}");
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }
        //刪除列
        public void DeleteRow(string table, string[] condition) {
            try {
                string commandStr = $@"DELETE FROM {table}
                    WHERE {String.Join(" and ", condition)};";
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
                sqlCommand.ExecuteNonQuery();

                //Console.WriteLine($"SQL: Delete Row, table: {table}");
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        //select
        public DataTable Select(string table) {
            var emptyStringArray = new string[] { };
            return Select(table, emptyStringArray, emptyStringArray);
        }
        public DataTable Select(string table, IEnumerable<string> column, string other = "") {
            var emptyStringArray = new string[] { };
            return Select(table, column, emptyStringArray, other);
        }
        public DataTable Select(string table, IEnumerable<string> column, IEnumerable<string> condition, string other = "") {
            try {
                string commandStr = $@"select " +
                    (column.Count() == 0 ? "*"
                    : string.Join(",", column)) +
                    $@" FROM {table}" +
                    (condition.Count() > 0 ?
                        $@" WHERE {String.Join(" and ", condition)}" : "") + " " +
                        other;
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
                DataTable dataTable = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);
                dataAdapter.Fill(dataTable);
                //Console.WriteLine($"SQL: Select, table: {table}");
                return dataTable;
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                return new DataTable();
            }
        }

        //select
        public DataTable SelectDistinct(string table, string[] column, string other = "") {
            var emptyStringArray = new string[] { };
            return SelectDistinct(table, column, emptyStringArray, other);
        }
        public DataTable SelectDistinct(string table, string[] column, string[] condition, string other = "") {
            try {
                string commandStr = $@"select distinct {string.Join(",", column)} FROM {table}" +
                    (condition.Length > 0 ?
                        $@" WHERE {String.Join(" and ", condition)}" : "") +
                        " " + other;
                SqlCommand sqlCommand = new SqlCommand(commandStr, connection);
                sqlCommand.CommandTimeout = 0;
                DataTable dataTable = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);
                dataAdapter.Fill(dataTable);
                //Console.WriteLine($"SQL: Select, table: {table}");
                return dataTable;
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                return new DataTable();
            }
        }
    }

    class SqlColumn
    {
        public string name = "defaultName";
        public string type = "smallint";
        public SqlDbType sqlDbType;
        public DbType dbType {
            get {
                return
                    this.type.StartsWith("char") ? DbType.String
                    : this.type.StartsWith("nchar") ? DbType.String
                    : this.type.StartsWith("decimal") ? DbType.Decimal
                    : this.type == "date" ? DbType.Date
                    : this.type.StartsWith("smalldatetime") ? DbType.DateTime
                    : this.type.StartsWith("bit") ? DbType.Boolean
                    : DbType.String;
            }
        }
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
        public List<object[]> DataList = new List<object[]>();
        public List<string> primaryKeys = new List<string>();

        public SqlInsertData() {
        }
        public SqlInsertData(List<SqlColumn> columnList) {
            this.ColumnList = columnList;
        }
        public void AddColumn(SqlColumn sqlColumn) {
            ColumnList.Add(sqlColumn);
        }
        public string GetInsertQuery(string table) {
            if (primaryKeys.Count > 0) {
                return $@"begin tran
                if exists(select * from {table} where {String.Join(" and ", primaryKeys.Select(x => x + "=@" + x))})
                begin
                   update {table} set {String.Join(",", ColumnList.Select(x => x.name + "=@" + x.name))} 
                   where {String.Join(" and ", primaryKeys.Select(x => x + "=@" + x))}
                end
                else
                begin
                   insert into {table}({String.Join(",", ColumnList.Select(x => x.name))})
                   values ({String.Join(",", ColumnList.Select(x => "@" + x.name))})
                end
                commit tran";
            } else {
                return $@"insert into {table}({String.Join(",", ColumnList.Select(x => x.name))}) 
                   values({ String.Join(",", ColumnList.Select(x => "@" + x.name))})";
            }
        }
        public void AddCmdParameters(SqlCommand sqlCommand) {
            foreach (var Column in ColumnList) {
                sqlCommand.Parameters.Add(new SqlParameter("@" + Column.name, Column.sqlDbType));
            }
        }
        public void AddData(object[] add) {
            DataList.Add(add);
        }
    }
}