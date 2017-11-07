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
                sql.DropTable("testTable1");
                sql.CreateTable("testTable1", new SqlColumn[] {
                    new SqlColumn("f1","int",false),
                    new SqlColumn("f2","int",false)
                });
                sql.AddColumn("testTable1", new SqlColumn("f3", "nchar(20)", true));
                //sql.SetPrimaryKey("testTable1","f1");
                SqlInsertData insertData = new SqlInsertData();
                insertData.AddColumn("f1", SqlDbType.Int);
                insertData.AddColumn("f2", SqlDbType.Int);
                insertData.AddColumn("f3", SqlDbType.Char);
                insertData.AddData(new Object[] { 1, 2, "s" });
                insertData.AddData(new Object[] { 3, 4, 3 });
                insertData.AddData(new Object[] { 5, 3, "你好" });
                sql.Insert("testTable1", insertData);
                Console.ReadKey(false);
            }
        }
    }
}
