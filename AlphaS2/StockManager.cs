using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class StockManager {

        static int[] DAYS = new int[] { 3, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60 };

    
        public static void Initialize() {
            InitializeStockList();
            InitializeFetchLog();
            InitializeLevel1();
            InitializeLevel2();
            InitializeLevel3();
        }

        static void InitializeStockList() {
            using (Sql sql = new Sql()) {
                sql.CreateTable("stock_list", new SqlColumn[] {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("name","nchar(20)",false),
                    new SqlColumn("type","char(1)",false),
                    new SqlColumn("start_date","datetime",true),
                    new SqlColumn("end_date","datetime",true)
                });
                sql.SetPrimaryKey("stock_list", "id");
            }
        }

        static void InitializeFetchLog() {
            using (Sql sql = new Sql()) {
                sql.CreateTable("fetch_log", new SqlColumn[] {
                    new SqlColumn("type","char(1)",false),
                    new SqlColumn("fetch_date","datetime",false),
                    new SqlColumn("empty","bit",false),
                    new SqlColumn("uploaded","bit",false)
                });
                sql.SetPrimaryKeys("fetch_log", new string[] { "type" ,"fetch_date"});
            }
        }

        static void InitializeLevel1() {
            using (Sql sql = new Sql()) {
                sql.CreateTable("level1", new SqlColumn[] {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("date","date",false),
                    new SqlColumn("deal","decimal(19,0)",false),
                    new SqlColumn("amount","decimal(19,0)",false),
                    new SqlColumn("price_open","decimal(9,2)",false),
                    new SqlColumn("price_close","decimal(9,2)",false),
                    new SqlColumn("price_high","decimal(9,2)",false),
                    new SqlColumn("price_low","decimal(9,2)",false),
                    new SqlColumn("difference","decimal(9,2)",false),
                    new SqlColumn("trade","decimal(9,0)",false)
                });
                sql.SetPrimaryKeys("level1", new string[] { "id", "date" });
            }
        }
        static void InitializeLevel2() {
            using (Sql sql = new Sql()) {
                sql.CreateTable("level2", new SqlColumn[] {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("date","date",false),
                    new SqlColumn("amount_per_trade","decimal(19,0)",false),
                    new SqlColumn("divide","decimal(9,4)",false),
                    new SqlColumn("fix","decimal(9,4)",false),
                    new SqlColumn("price_mean","decimal(9,2)",false),
                    new SqlColumn("Nprice_mean","decimal(9,2)",false),
                    new SqlColumn("Nprice_open","decimal(9,2)",false),
                    new SqlColumn("Nprice_close","decimal(9,2)",false),
                    new SqlColumn("Nprice_high","decimal(9,2)",false),
                    new SqlColumn("Nprice_low","decimal(9,2)",false)
                });
                sql.SetPrimaryKeys("level2", new string[] { "id", "date" });
            }
        }
        static void InitializeLevel3() {
            using (Sql sql = new Sql()) {
                var newColumns = new List<SqlColumn>() {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("date","date",false)
                };
                foreach (var c in new string[] { "mean", "close", "cost" , "volume"}) { 
                    foreach (var d in DAYS) {
                        newColumns.Add(new SqlColumn($@"ma_{c}_mean_{d}", "decimal(9,2)", false));
                    }
                }

                sql.CreateTable("level3", newColumns);
                sql.SetPrimaryKeys("level3", new string[] { "id", "date" });
            }
        }


        public static void DropAllList() {
            DropLevel3();
            DropLevel2();
            DropLevel1();
            DropFetchLog();
            DropStockList();
        }
        static void DropStockList() {
            using (Sql sql = new Sql()) {
                sql.DropTable("stock_list");
            }
        }
        static void DropFetchLog() {
            using (Sql sql = new Sql()) {
                sql.DropTable("fetch_log");
            }
        }
        static void DropLevel1() {
            using (Sql sql = new Sql()) {
                sql.DropTable("level1");
            }
        }
        static void DropLevel2() {
            using (Sql sql = new Sql()) {
                sql.DropTable("level2");
            }
        }
        static void DropLevel3() {
            using (Sql sql = new Sql()) {
                sql.DropTable("level3");
            }
        }

    }
}
