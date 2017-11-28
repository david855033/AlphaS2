using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace AlphaS2
{
    class Program
    {
        static void Main(string[] args) {
            using (Sql sql = new Sql()) {
                StockManager.DropAllList();
                StockManager.Initialize();

                //sql.DropTable("testTable1");
                //sql.CreateTable("testTable1", new SqlColumn[] {
                //    new SqlColumn("f1","int",false),
                //    new SqlColumn("f2","int",false)
                //});
                //sql.AddColumn("testTable1", new SqlColumn("f3", "nchar(20)", true));
                //sql.SetPrimaryKey("testTable1", "f1");
                //SqlInsertData insertData = new SqlInsertData();
                //insertData.AddColumn("f1", SqlDbType.Int);
                //insertData.AddColumn("f2", SqlDbType.Int);
                //insertData.AddColumn("f3", SqlDbType.Char);
                //insertData.AddData(new Object[] { 1, 2, "s" });
                //insertData.AddData(new Object[] { 3, 4, 3 });
                //insertData.AddData(new Object[] { 5, 3, "你好" });
                //sql.InsertRow("testTable1", insertData);
                //sql.SetConstraintPrimaryKey("testTable1", new[] { "f1", "f2" });
                //sql.DropPrimaryKey("testTable1");
                //var setValues = new Dictionary<string, string>();
                //setValues.Add("f2", "22222");
                //sql.UpdateRow("testTable1", setValues, new string[] { "f1=5" });
                //sql.DeleteRow("testTable1", new string[] { "f1=3" });
                Console.ReadKey(false);
            }
        }
    }
}
