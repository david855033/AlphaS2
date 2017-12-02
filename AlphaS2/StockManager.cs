using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class StockManager
    {

        static int[] DAYS = new int[] { 3, 5, 10, 15, 20, 30, 40, 50, 60 };
        static int[] DAYS_MACD = new int[] { 10, 20, 40, 60 };
        static int[] DAYS_KD = new int[] { 5, 10, 20, 40, 60 };
        static int[] DAYS_RSI = new int[] { 10, 20, 60 };

        public static void Initialize() {
            InitializeStockList();
            InitializeFetchLog();
            InitializeLevel1();
            InitializeLevel2();
            InitializeLevel3();
            InitializeLevel4();
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
                sql.SetPrimaryKeys("fetch_log", new string[] { "type", "fetch_date" });
            }
        }
        //level 1為原始資料
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
        //level 2為還原除權息後資料
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
        //level3 為近日計算資料(如平均線、N日內極大極小值)
        static void InitializeLevel3() {
            using (Sql sql = new Sql()) {
                var newColumns = new List<SqlColumn>() {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("date","date",false)
                };
                //mean average
                foreach (var c in new string[] { "mean", "cost", "volume" }) {
                    foreach (var d in DAYS) {
                        newColumns.Add(new SqlColumn($@"ma_{c}_{d}", "decimal(9,2)", false));
                    }
                }

                //MACD-dif: 不同條moving average間之差異 使用mean
                //MACD-dem: dif之10日ma (僅在level3)
                //MACD: def-dem  (僅在level4)
                foreach (var d1 in DAYS_MACD) {
                    foreach (var d2 in DAYS_MACD.Where(x => x > d1)) {
                        newColumns.Add(new SqlColumn($@"dif_{d1}_{d2}", "decimal(9,2)", false));
                        newColumns.Add(new SqlColumn($@"dem_{d1}_{d2}", "decimal(9,2)", false));
                    }
                }

                //max min極大極小值
                foreach (var c in new string[] { "price" }) {
                    foreach (var d in DAYS_KD) {
                        newColumns.Add(new SqlColumn($@"max_{c}_{d}", "decimal(9,2)", false));
                        newColumns.Add(new SqlColumn($@"min_{c}_{d}", "decimal(9,2)", false));
                    }
                }
                sql.CreateTable("level3", newColumns);
                sql.SetPrimaryKeys("level3", new string[] { "id", "date" });
            }
        }

        //level4 為進入計算的資料
        static void InitializeLevel4() {
            using (Sql sql = new Sql()) {
                var newColumns = new List<SqlColumn>() {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("date","date",false)
                };
                //BA 當日與指定時間與moving average的差距，換算成log10
                foreach (var c in new string[] { "mean", "volume" }) {
                    foreach (var d in DAYS) {
                        newColumns.Add(new SqlColumn($@"ba_{c}_{d}", "decimal(9,2)", false));
                    }
                }

                //dif_cost_mean(一段時間的交易成本cost與每日平均交易價mean的差距)，換算成log10
                foreach (var d in DAYS) {
                    newColumns.Add(new SqlColumn($@"dif_cost_mean{d}", "decimal(9,2)", false));
                }

                //MACD-dif: 不同條moving average間之差異 使用mean
                //MACD-dem: dif之10日ma (僅在level3)
                //MACD: def-dem  (僅在level4)
                foreach (var d1 in DAYS_MACD) {
                    foreach (var d2 in DAYS_MACD.Where(x => x > d1)) {
                        newColumns.Add(new SqlColumn($@"dif_{d1}_{d2}", "decimal(9,2)", false));
                        newColumns.Add(new SqlColumn($@"macd_{d1}_{d2}", "decimal(9,2)", false));
                    }
                }

                //RSV KD (平滑率=1/3), J=K-D
                foreach (var c in new string[] { "rsv", "k", "d", "j" }) {
                    foreach (var d in DAYS_KD) {
                        newColumns.Add(new SqlColumn($@"{c}_{d}", "decimal(9,2)", false));
                    }
                }

                //RSI
                foreach (var d in DAYS_RSI) {
                    newColumns.Add(new SqlColumn($@"rsi_{d}", "decimal(9,2)", false));
                }
                foreach (var d1 in DAYS_RSI) {
                    foreach (var d2 in DAYS_RSI.Where(x => x > d1)) {
                        newColumns.Add(new SqlColumn($@"rsi_ba_{d1}_{d2}", "decimal(9,2)", false));
                    }
                }

                sql.CreateTable("level4", newColumns);
                sql.SetPrimaryKeys("level4", new string[] { "id", "date" });
            }
        }

        public static void DropAllList() {
            DropLevel4();
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

        static void DropLevel4() {
            using (Sql sql = new Sql()) {
                sql.DropTable("level4");
            }
        }
    }
}
