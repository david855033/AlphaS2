using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class StockManager
    {
        public static void Initialize() {
            InitializeList();
            InitializeFetchLog();
            InitializeLevel1();
        }

        static void InitializeList() {
            using (Sql sql = new Sql()) {
                sql.CreateTable("stock_list", new SqlColumn[] {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("name","nchar(20)",false),
                    new SqlColumn("type","char(1)",false),
                    new SqlColumn("last_fetch","datetime",false)
                }); 
            }
        }

        static void InitializeFetchLog() {
            using (Sql sql = new Sql()) {
                sql.CreateTable("fetch_log", new SqlColumn[] {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("fetch_date","datetime",false),
                    new SqlColumn("finish","bit",false)
                });
            }
        }

        static void InitializeLevel1() {
            using (Sql sql = new Sql()) {
                sql.CreateTable("level1", new SqlColumn[] {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("date","date",false),
                    new SqlColumn("deal","decimal(19,0)",false),
                    new SqlColumn("amount","decimal(19,0)",false),
                    new SqlColumn("open","decimal(9,2)",false),
                    new SqlColumn("close","decimal(9,2)",false),
                    new SqlColumn("high","decimal(9,2)",false),
                    new SqlColumn("low","decimal(9,2)",false),
                    new SqlColumn("difference","decimal(9,2)",false),
                    new SqlColumn("trade_count","decimal(9,0)",false)
                });
            }
        }
    }
}
