using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class StockManager
    {
        static int[] DAYS = new int[] { 3, 5, 10, 15, 20, 30, 40, 50, 60 };
        static int[] DAYS_MACD = new int[] { 10, 20, 40, 60 };
        static int[] DAYS_KD = new int[] { 5, 10, 20, 40, 60 };
        static int[] DAYS_RSI = new int[] { 10, 20, 60 };
        static int[] DAYS_DMI = new int[] { 10, 20, 60 };
        static int[] DAYS_FP = new int[] { 5, 10, 20, 30, 40, 50, 60, 70, 80 };
        static int[] DAYS_FR = new int[] { 20, 40, 60 };

        public static void Initialize() {
            InitializeStockList();
            InitializeLevel2();
            InitializeLevel3();
            InitializeLevel4();
            InitializeLevel5();
            LoadStockList();
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

        static void LoadStockList() {
            using (Sql sql = new Sql()) {
                var inserData = new SqlInsertData();
                inserData.AddColumn(new SqlColumn("id", System.Data.SqlDbType.NChar));
                inserData.AddColumn(new SqlColumn("name", System.Data.SqlDbType.NChar));
                inserData.AddColumn(new SqlColumn("type", System.Data.SqlDbType.Char));
                inserData.AddData(new object[] { "0050", "台灣50", "A" });
                inserData.AddData(new object[] { "1101", "台泥", "A" });
                sql.InsertUpdateRow("stock_list", inserData);
            }
        }

        //level 1為原始資料
        public static void InitializeLevel1() {
            using (Sql sql = new Sql()) {
                sql.CreateTable("level1", Level1.column);
                sql.SetPrimaryKeys("level1", new string[] { "id", "date" });
            }
        }
        public static void GenerateLevel1() {
            List<FetchLog> fetchLog = FetchLogManager.GetFileListToUpload();

            Dictionary<string, string> lastCloseSet = new Dictionary<string, string>();

            foreach (var f in fetchLog) {
                string filePath = f.FilePath;
                string dateString = filePath.Split('\\').Last().Split('_').Last().Split('.').First().Insert(6, "-").Insert(4, "-");
                Console.WriteLine($@"loading  to level1: {filePath}");
                SqlInsertData insertData = new SqlInsertData(Level1.column);
                insertData.primaryKeys.Add("id");
                insertData.primaryKeys.Add("date");
                using (var sr = new StreamReader(filePath, Encoding.Default)) {
                    string[] content = sr.ReadToEnd().Split('\n');
                    //選取股票資料行
                    string pattern = @"\A(=?\""\d{4,}\D?\"",)";
                    string[] stockData = content
                        .Where(x => Regex.IsMatch(x, pattern))
                        .Select(x => x.TrimEnd(new[] { ',', '\r' })).ToArray();

                    foreach (var row in stockData) {
                        //去除引號內逗點並以陣列回傳各欄位資料
                        var dataFields =
                            Regex.Matches(row, @"=?"".*?""")
                            .Cast<Match>()
                            .Select(x => x.Value.Replace(",", "").Replace("\"", "")).ToArray();

                        string id = dataFields[0].Trim();
                        string deal="", amount = "", price_open = "", price_close = "", nonZeroClose = "", price_high = "", price_low = "", price_ref_nextday = "", difference = "", trade = "";

                        if (f.type == 'A') {
                            deal = dataFields[2];   //deal
                            amount = dataFields[4];   //amount
                            price_open = (dataFields[5] == "--" ? "0" : dataFields[5]); //open
                            price_close = (dataFields[8] == "--" ? "0" : dataFields[8]); //close
                            price_high = (dataFields[6] == "--" ? "0" : dataFields[6]); //high
                            price_low = (dataFields[7] == "--" ? "0" : dataFields[7]); //low
                            price_ref_nextday = "0"; //refnextday
                            difference = (dataFields[9] == "-" ? "-" : "") + (dataFields[10] == "-" ? "0" : dataFields[10]); //dif
                            trade = dataFields[3];
                        } else if (f.type == 'B') {
                            deal = dataFields[8]; //deal
                            amount = dataFields[9]; //amount
                            price_open = dataFields[4];  //open
                            price_close = dataFields[2];  //close
                            price_high = dataFields[5]; //high
                            price_low = dataFields[6]; //low
                            price_ref_nextday = dataFields[14];  //refnextday
                            difference = "0"; //dif
                            trade = dataFields[10];//trade
                        }

                        //如果price_close不為零，設定non zero close
                        if (Convert.ToDecimal(price_close) > 0) {
                            nonZeroClose = price_close;
                        } else {
                            //如果price_close為0
                            //搜尋last close set，如果有資料就當作nonZeroClose
                            if (lastCloseSet.TryGetValue(id, out string tryGetValue)) {
                                nonZeroClose = tryGetValue;
                            } else {
                                //如果last close set沒資料，用sql搜尋上一筆non zero close
                                using (Sql sql = new Sql()) {
                                    var lastCloseQuery = sql.Select("level1",
                                        new string[] { "top 1 price_close_nonzero" },
                                        new string[] { $"id = '{id}'" },
                                        "order by date desc");
                                    if (lastCloseQuery.Rows.Count > 0) {
                                        nonZeroClose = lastCloseQuery.Rows[0][0].ToString();
                                    } else {
                                        //如果還是沒資料，nonZeroClose設定為10
                                        nonZeroClose = "10";
                                    }
                                }
                            }
                        }
                        //將close推入
                        lastCloseSet[id] = nonZeroClose;
                       
                        insertData.AddData(new object[] {
                            id, //id
                            dateString,   //date
                            deal,   //deal
                            amount,   //amount
                            price_open,
                            price_close, //close
                            nonZeroClose, //non zero close
                            price_high,
                            price_low,
                            price_ref_nextday, //refnextday
                            difference,
                            trade
                        });
                    }
                    using (Sql sql = new Sql()) {
                        var successInsert = sql.InsertUpdateRow("level1", insertData);
                        if (successInsert) {
                            sql.UpdateRow("fetch_log",
                              new Dictionary<string, string>() { { "uploaded", "1" } },
                              new string[] { $"type = '{f.type}'", $"date = '{dateString}'" });
                        }
                    }
                }
            }
        }

        //level 2為還原除權息後資料
        public static void InitializeLevel2() {
            using (Sql sql = new Sql()) {
                sql.CreateTable("level2", Level2.column);
                sql.SetPrimaryKeys("level2", new string[] { "id", "date" });
            }
        }
        public static void GenerateLevel2() {
            List<string> IDList = GetIDListLevel1();
            List<DateTime> dateList = GetDateListFetchLog();
            Console.WriteLine($"Generating Level2, available id = {IDList.Count}");
            using (Sql sql = new Sql()) {
                int currentLineCursor = Console.CursorTop;
                int count = 0;
                foreach (string id in IDList) {
                    //找尋level2內最後的一天
                    String maxDateLevel2Str = GetLastDate(sql, "level2", id);
                    //搜尋這一天的對應datelist中的index(startDateIndex)
                    int startDateIndex = 0;
                    if (maxDateLevel2Str != "") {
                        DateTime maxDateLevel2 = Convert.ToDateTime(maxDateLevel2Str);
                        startDateIndex = dateList.FindIndex(x => x == maxDateLevel2);
                    }
                    //選取Level1內資料(含level2資料最後一天，及之後的資料)
                    DateTime level1SearchAfterDate = dateList[startDateIndex];
                    DataTable dataTableLevel1 = sql.Select("level1",
                        Level1.column.Select(x => x.name).ToArray(),
                        new string[] { $"id='{id}'",
                            $"date >= '{level1SearchAfterDate.ToString("yyyy-MM-dd")}'" }
                        );
                    List<Level1> level1Data = Level1.DataAdaptor(dataTableLevel1);
                    //選取level2最後一筆fix (預設為1)
                    var lastFixQuery = sql.Select("level2",
                        new string[] { "top 1 fix" },
                         new string[] { $"id='{id}'" }, "order by date desc");
                    Decimal lastFix = 1;
                    if (lastFixQuery.Rows.Count > 0) {
                        lastFix = Convert.ToDecimal(lastFixQuery.Rows[0][0]);
                    }

                    //建立初始 上一筆資料 (level2的fix以及level1的close) 用來處理交易量為0之情況
                    Level2 lastLevel2Data = new Level2() { fix = lastFix };
                    decimal lastLevel1Close = 0;
                    if (startDateIndex > 0) {
                        lastLevel1Close = level1Data.First().price_close;
                    }

                    //將每筆level1算出level2 並更新sql  
                    List<Level2> level2DataToInsert = new List<Level2>();
                    for (int i = startDateIndex; i < dateList.Count; i++) {
                        var thisLevel2Data = new Level2() {
                            id = level1Data[i].id,
                            date = level1Data[i + startDateIndex].date
                        };

                        //判斷divide
                        decimal realDifference;
                        if (level1Data[i].price_ref_nextday == 0) {
                            //A組資料 隔日參考價=0
                            realDifference = level1Data[i].price_close - level1Data[i - 1].price_close;
                            thisLevel2Data.divide = level1Data[i].difference - realDifference;
                        } else {
                            //B組資料 有隔日參考價
                            realDifference = 0;
                        }

                        if (i == 0) {
                            thisLevel2Data.divide = 0;
                            thisLevel2Data.fix = lastFix;
                        } else {
                            thisLevel2Data.fix = lastFix *
                                (level1Data[i - 1].price_close + thisLevel2Data.divide) /
                                level1Data[i - 1].price_close;

                            lastFix = thisLevel2Data.fix;
                        }


                        thisLevel2Data.Nprice_mean = thisLevel2Data.price_mean;
                        thisLevel2Data.Nprice_open = level1Data[i].price_open;
                        thisLevel2Data.Nprice_close = level1Data[i].price_close;
                        thisLevel2Data.Nprice_high = level1Data[i].price_high;
                        thisLevel2Data.Nprice_low = level1Data[i].price_low;


                        //平均價格 => 若成交量為0，平均價格=收盤價格
                        if (level1Data[i].deal > 0) {
                            thisLevel2Data.price_mean = level1Data[i].amount / level1Data[i].deal;
                        } else if (i > 0) {
                            thisLevel2Data.price_mean = lastLevel2Data.price_mean;
                        } else {
                            thisLevel2Data.price_mean = level1Data[0].price_close;
                        }




                        level2DataToInsert.Add(thisLevel2Data);
                        lastLevel2Data = thisLevel2Data;
                    }
                    sql.InsertUpdateRow("level2", Level2.GetInsertData(level2DataToInsert));
                    Console.SetCursorPosition(0, currentLineCursor);
                    Console.WriteLine($@"generate level 2 id: {id}  ({++count}/{IDList.Count})       ");
                }
            }
        }
        ////傳回level1內所含的ID(distinct)
        static List<string> GetIDListLevel1() {
            var result = new List<string>();
            using (Sql sql = new Sql()) {
                var resultTable = sql.SelectDistinct("level1",
                    new string[] { "id" }, "order by id");
                for (int i = 0; i < resultTable.Rows.Count; i++) {
                    result.Add(resultTable.Rows[i][0].ToString().Trim());
                }
            }
            return result;
        }
        //傳回fetch_log內所含的date(dinstinct)
        static List<DateTime> GetDateListFetchLog() {
            var result = new List<DateTime>();
            using (Sql sql = new Sql()) {
                var resultTable = sql.SelectDistinct("fetch_log",
                   new string[] { "date" },
                   new string[] { "empty = 0" },
                   "order by date");
                for (int i = 0; i < resultTable.Rows.Count; i++) {
                    result.Add(Convert.ToDateTime(resultTable.Rows[i][0].ToString().Trim()));
                }
            }
            return result;
        }

        //取得某個table-id資料最後一筆的日期(以string回傳)
        static string GetLastDate(Sql sql, string table, string id) {
            string lastDate = "";
            var maxDateSQuery = sql.Select("level2",
                       new string[] { "top 1 date" },
                        new string[] { $"id='{id}' order by date desc" });
            if (maxDateSQuery.Rows.Count > 0) {
                lastDate = maxDateSQuery.Rows[0][0].ToString();
            }
            return lastDate;
        }

        //level3 為近日計算資料(如平均線、N日內極大極小值)
        public static void InitializeLevel3() {
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

                //max min極大極小值 for KDJ
                foreach (var c in new string[] { "price" }) {
                    foreach (var d in DAYS_KD) {
                        newColumns.Add(new SqlColumn($@"max_{c}_{d}", "decimal(9,2)", false));
                        newColumns.Add(new SqlColumn($@"min_{c}_{d}", "decimal(9,2)", false));
                    }
                }

                //DMI
                //posdm = 今日最高-昨日最高(只取正值)
                //negdm = 昨日最低-今日最低(只取正值)
                //tr = max(H - L, H-C(t-1), L-C(t-1))
                foreach (var c in new string[] { "posdm", "negdm", "tr" }) {
                    foreach (var d in DAYS_DMI) {
                        newColumns.Add(new SqlColumn($@"{c}_{d}", "decimal(9,2)", false));
                    }
                }

                sql.CreateTable("level3", newColumns);
                sql.SetPrimaryKeys("level3", new string[] { "id", "date" });
            }
        }
        //level4 為進入計算的資料
        public static void InitializeLevel4() {
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
                //DMI
                //posDI = posdm / tr
                //negDI = negdm / tr
                //dx = ABS((posDI-negDI)/(posDI+negDI))
                //adx = ema(dx)
                //adxr = ema(adx)
                foreach (var c in new string[] { "posdi", "negdi", "dx", "adx", "adxr" }) {
                    foreach (var d in DAYS_DMI) {
                        newColumns.Add(new SqlColumn($@"{c}_{d}", "decimal(9,2)", false));
                    }
                }
                //W%R = Hn-Cn / Hn - Ln
                foreach (var c in new string[] { "wr" }) {
                    foreach (var d in DAYS_KD) {
                        newColumns.Add(new SqlColumn($@"{c}_{d}", "decimal(9,2)", false));
                    }
                }

                sql.CreateTable("level4", newColumns);
                sql.SetPrimaryKeys("level4", new string[] { "id", "date" });
            }
        }
        //level 5 = Future Price
        public static void InitializeLevel5() {
            using (Sql sql = new Sql()) {
                var newColumns = new List<SqlColumn>() {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("date","date",false)
                };
                foreach (var d in DAYS_FP) {
                    newColumns.Add(new SqlColumn($@"future_price_{d}", "decimal(9,2)", false));
                }
                foreach (var d in DAYS_FR) {
                    newColumns.Add(new SqlColumn($@"future_rank_{d}", "decimal(9,2)", false));
                }
                sql.CreateTable("level5", newColumns);
                sql.SetPrimaryKeys("level5", new string[] { "id", "date" });
            }
        }

        public static void DropAllList() {
            DropLevel5();
            DropLevel4();
            DropLevel3();
            DropLevel2();
            DropStockList();
        }
        public static void DropStockList() {
            using (Sql sql = new Sql()) {
                sql.DropTable("stock_list");
            }
        }

        public static void DropLevel1() {
            using (Sql sql = new Sql()) {
                sql.DropTable("level1");
            }
        }
        public static void DropLevel2() {
            using (Sql sql = new Sql()) {
                sql.DropTable("level2");
            }
        }
        public static void DropLevel3() {
            using (Sql sql = new Sql()) {
                sql.DropTable("level3");
            }
        }
        public static void DropLevel4() {
            using (Sql sql = new Sql()) {
                sql.DropTable("level4");
            }
        }
        public static void DropLevel5() {
            using (Sql sql = new Sql()) {
                sql.DropTable("level5");
            }
        }
    }
}

