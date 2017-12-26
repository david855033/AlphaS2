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

        public static void Initialize() {
            Level3.Initiate();
            Level4.Initiate();
            InitializeStockList();
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
            int currentLineCursor = Console.CursorTop;
            foreach (var f in fetchLog) {
                Console.SetCursorPosition(0, currentLineCursor);
                if (f.type == 'A' || f.type == 'B') {
                    UpdateAB(lastCloseSet, f);
                } else if (f.type == 'Z') {
                    UpdateZ(f);
                }
            }
        }
        private static void UpdateAB(Dictionary<string, string> lastCloseSet, FetchLog f) {
            string filePath = f.FilePath;
            string dateString = filePath.Split('\\').Last().Split('_').Last().Split('.').First().Insert(6, "-").Insert(4, "-");
            Console.WriteLine($@"loading  to level1: {filePath}           ");
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
                    string deal = "", amount = "", price_open = "", price_close = "", nonZeroClose = "", price_high = "", price_low = "", price_ref_nextday = "", trade = "";

                    if (f.type == 'A') {
                        deal = dataFields[2];   //deal
                        amount = dataFields[4];   //amount
                        price_open = (dataFields[5] == "--" ? "0" : dataFields[5]); //open
                        price_close = (dataFields[8] == "--" ? "0" : dataFields[8]); //close
                        price_high = (dataFields[6] == "--" ? "0" : dataFields[6]); //high
                        price_low = (dataFields[7] == "--" ? "0" : dataFields[7]); //low
                        price_ref_nextday = "0"; //refnextday
                        trade = dataFields[3];
                    } else if (f.type == 'B') {
                        deal = dataFields[8]; //deal
                        amount = dataFields[9]; //amount
                        price_open = dataFields[4].IndexOf("---") >= 0 ? "0" : dataFields[4];  //open
                        price_close = dataFields[2].IndexOf("---") >= 0 ? "0" : dataFields[2];  //close
                        price_high = dataFields[5].IndexOf("---") >= 0 ? "0" : dataFields[5]; //high
                        price_low = dataFields[6].IndexOf("---") >= 0 ? "0" : dataFields[6]; //low
                        price_ref_nextday = dataFields[14].IndexOf("---") >= 0 ? "0" : dataFields[14]; ;  //refnextday
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
                            0,
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
        private static void UpdateZ(FetchLog f) {
            string filePath = f.FilePath;
            string dateString = filePath.Split('\\').Last().Split('_').Last().Split('.').First().Insert(6, "-").Insert(4, "-");
            Console.WriteLine($@"loading to level1, for divide in A: {filePath}       ");
            using (var sr = new StreamReader(filePath, Encoding.Default))
            using (Sql sql = new Sql()) {
                string[] content = sr.ReadToEnd().Split('\n');
                //選取股票資料行
                string[] stockData = content
                    .Where(x => x.StartsWith("="))
                    .Select(x => x.TrimEnd(new[] { ',', '\r' })).ToArray();
                string title = content
                    .Where(x => x.Contains("權值+息值"))
                    .Where(x => x.Contains("資料日期")).First();
                var TitleField =
                        Regex.Matches(title, @"=?"".*?""")
                        .Cast<Match>()
                        .Select(x => x.Value.Replace(",", "").Replace("\"", "")).ToList();
                int divideColIndex = TitleField.FindIndex(x => x.Contains("權值+息值"));
                foreach (var row in stockData) {
                    //去除引號內逗點並以陣列回傳各欄位資料
                    var dataFields =
                        Regex.Matches(row, @"=?"".*?""")
                        .Cast<Match>()
                        .Select(x => x.Value.Replace(",", "").Replace("\"", "")).ToArray();
                    string id = dataFields[1], divide = dataFields[divideColIndex];
                    //查詢除權息日期(大於等於dateString的最小日期) 避免颱風假等狀況
                    var dateStringQuery = sql.Select("level1",
                        new string[] { "min(date)" },
                        new string[] { $"id = '{id}'", $"date >= '{dateString}'" });
                    string newDateString = dateString;
                    if (dateStringQuery.Rows.Count > 0) {
                        newDateString = Convert.ToDateTime(dateStringQuery.Rows[0][0]).ToString("yyyy-MM-dd");
                    }
                    var successInsert = sql.UpdateRow("level1",
                        new Dictionary<string, string>() { { "divide", divide } },
                        new string[] { $"id = '{id}'", $"date = '{newDateString}'" });
                    if (successInsert) {
                        sql.UpdateRow("fetch_log",
                            new Dictionary<string, string>() { { "uploaded", "1" } },
                            new string[] { $"type = '{f.type}'", $"date = '{dateString}'" });
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
            Console.WriteLine($"Calculating Level2, available id = {IDList.Count}");
            using (Sql sql = new Sql()) {
                int currentLineCursor = Console.CursorTop;
                int count = 0;
                foreach (string id in IDList) {
                    //找尋level2內最後的一天
                    String maxDateLevel2Str = GetLastDate(sql, "level2", id);
                    //搜尋這一天的對應datelist中的index(startDateIndex)
                    int startDateIndex = -1;
                    if (maxDateLevel2Str != "") {
                        DateTime maxDateLevel2 = Convert.ToDateTime(maxDateLevel2Str);
                        startDateIndex = dateList.FindIndex(x => x == maxDateLevel2);
                    }
                    //選取level1內資料(level2最後一天之後(不包含)=要新增的level2資料)
                    string dateCondition = startDateIndex >= 0 ?
                        $"date > '{dateList[startDateIndex].ToString("yyyy-MM-dd")}'" :
                        $"date >= '{GlobalSetting.START_DATE.ToString("yyyy-MM-dd")}'";   //含第一筆
                    DataTable dataTableLevel1 = sql.Select("level1",
                        Level1.column.Select(x => x.name).ToArray(),
                        new string[] { $"id='{id}'",
                             dateCondition}
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
                    //選取level1最後一筆price_close_non zero / lastRefNextDay
                    var lastLevel1Query = sql.Select("level1",
                        new string[] { "top 1 price_close_nonzero", "price_ref_nextday" },
                         new string[] { $"id='{id}'" }, "order by date desc");
                    Decimal lastCloseNonZero = 10, lastRefNextDay = 10;
                    if (lastLevel1Query.Rows.Count > 0) {
                        lastCloseNonZero = Convert.ToDecimal(lastLevel1Query.Rows[0][0]);
                        lastRefNextDay = Convert.ToDecimal(lastLevel1Query.Rows[0][1]);
                    }

                    //將每筆level1算出level2 並更新sql  
                    List<Level2> level2DataToInsert = new List<Level2>();
                    for (int i = startDateIndex + 1; i < dateList.Count; i++) {
                        //先找出相同日期的level1 data
                        Level1 matchLevel1Data = level1Data.Find(x => x.date == dateList[i]);

                        Level2 thisLevel2Data = new Level2() {
                            id = id,
                            date = dateList[i],
                            Fix = lastFix,
                            amount_per_trade = 0,
                            Divide = 0,
                            Nprice_close = 0,
                            Nprice_high = 0,
                            Nprice_low = 0,
                            Nprice_mean = 0,
                            Nprice_open = 0,
                            Price_mean = 0
                        };


                        if (matchLevel1Data != null) {  //有找到level1data
                            //判斷divide
                            if (i > 0) {  //第二筆資料之後才需要算divide
                                if (matchLevel1Data.price_ref_nextday == 0) {
                                    //A組資料 直接採用當日divide
                                    thisLevel2Data.Divide = matchLevel1Data.divide;
                                } else {
                                    //B組資料 前一天的 隔日收盤價-參考價
                                    thisLevel2Data.Divide =
                                        lastCloseNonZero - lastRefNextDay;
                                }
                            }
                            //若divide>0, 且當天有交易量 將fix乘上倍數
                            if (thisLevel2Data.Divide > 0 &&
                                (matchLevel1Data.price_close_nonzero - thisLevel2Data.Divide) > 0 &&
                                matchLevel1Data.price_close > 0) {
                                thisLevel2Data.Fix *=
                                    matchLevel1Data.price_close_nonzero /
                                    (matchLevel1Data.price_close_nonzero - thisLevel2Data.Divide);
                            }

                            if (matchLevel1Data.trade > 0) {
                                thisLevel2Data.amount_per_trade = matchLevel1Data.amount / matchLevel1Data.trade;
                            }
                            if (matchLevel1Data.deal > 0) {
                                thisLevel2Data.Price_mean = matchLevel1Data.amount / matchLevel1Data.deal;
                                thisLevel2Data.Nprice_mean = thisLevel2Data.Price_mean * thisLevel2Data.Fix;
                            }

                            if (matchLevel1Data.price_close_nonzero > 0) {
                                thisLevel2Data.Nprice_close = matchLevel1Data.price_close_nonzero * thisLevel2Data.Fix;
                            }
                            if (matchLevel1Data.price_high > 0) {
                                thisLevel2Data.Nprice_high = matchLevel1Data.price_high * thisLevel2Data.Fix;
                            }
                            if (matchLevel1Data.price_low > 0) {
                                thisLevel2Data.Nprice_low = matchLevel1Data.price_low * thisLevel2Data.Fix;
                            }
                            if (matchLevel1Data.price_open > 0) {
                                thisLevel2Data.Nprice_open = matchLevel1Data.price_open * thisLevel2Data.Fix;
                            }

                            //將資料存到last data
                            lastCloseNonZero = matchLevel1Data.price_close_nonzero;
                            lastFix = thisLevel2Data.Fix;
                            lastRefNextDay = matchLevel1Data.price_ref_nextday;
                        }

                        level2DataToInsert.Add(thisLevel2Data);
                    }
                    sql.InsertUpdateRow("level2", Level2.GetInsertData(level2DataToInsert));
                    Console.SetCursorPosition(0, currentLineCursor);
                    Console.WriteLine($@"Calculate level 2 id: {id}  ({++count}/{IDList.Count})       ");
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
                   new string[] { "empty = 0", "type = 'A'" },
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
            var maxDateSQuery = sql.Select(table,
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
                var newColumns = Level3.column;
                sql.CreateTable("level3", newColumns);
                sql.SetPrimaryKeys("level3", new string[] { "id", "date" });
            }
        }
        public static void GenerateLevel3() {
            List<string> IDList = GetIDListLevel1();
            List<DateTime> dateList = GetDateListFetchLog();
            Console.WriteLine($"Calculating Level3, available id = {IDList.Count}");
            using (Sql sql = new Sql()) {
                int currentLineCursor = Console.CursorTop;
                int count = 0;
                foreach (string id in IDList) {
                    //找尋level3內最後的一天
                    String maxDateLevel3Str = GetLastDate(sql, "level3", id);
                    //搜尋這一天的對應datelist中的index(startDateIndex)
                    int startDateIndex = -1;
                    if (maxDateLevel3Str != "") {
                        DateTime maxDateLevel3 = Convert.ToDateTime(maxDateLevel3Str);
                        startDateIndex = dateList.FindIndex(x => x == maxDateLevel3);
                    }
                    //startDateIndex必須大於60
                    startDateIndex = Math.Max(60, startDateIndex);

                    //選取level2內資料(起點要回推60日) 至最新的資料
                    const int BACK_DAYS = 60;
                    string dateCondition = startDateIndex - BACK_DAYS >= 0 ?
                        $"date > '{dateList[startDateIndex - BACK_DAYS].ToString("yyyy-MM-dd")}'" :
                        $"date >= '{GlobalSetting.START_DATE.ToString("yyyy-MM-dd")}'";   //含第一筆
                    DataTable dataTableLevel2 = sql.Select("level2",
                        Level2.column.Select(x => x.name).ToArray(),
                        new string[] { $"id='{id}'",
                             dateCondition}
                        );
                    List<Level2> level2Data = Level2.DataAdaptor(dataTableLevel2);

                    //選取level1內資料 (起點要不用回推) 至最新的資料
                    dateCondition = startDateIndex >= 0 ?
                        $"date > '{dateList[startDateIndex].ToString("yyyy-MM-dd")}'" :
                        $"date >= '{GlobalSetting.START_DATE.ToString("yyyy-MM-dd")}'";   //含第一筆
                    DataTable dataTableLevel1 = sql.Select("level1",
                        Level1.column.Select(x => x.name).ToArray(),
                        new string[] { $"id='{id}'",
                             dateCondition}
                        );
                    List<Level1> level1Data = Level1.DataAdaptor(dataTableLevel1);


                    //選取level3最後一筆資料
                    var lastLevel3Query = sql.Select("level3",
                        new string[] { "top 1 *" },
                         new string[] { $"id='{id}'" }, "order by date desc");
                    List<Level3> lastLevel3Data = Level3.DataAdaptor(lastLevel3Query);
                    Level3 lastLevel3 = null;
                    if (lastLevel3Data.Count > 0) {
                        lastLevel3 = lastLevel3Data.First();
                    }

                    //將每筆level2算出level3 並更新sql 
                    List<Level3> level3DataToInsert = new List<Level3>();
                    for (int i = startDateIndex; i < dateList.Count; i++) {
                        Level1 matchedLevel1Data = level1Data.Find(x => x.date == dateList[i]);
                        if (matchedLevel1Data == null) {
                            continue;
                        }
                        Level2 matchedLevel2Data = level2Data.Find(x => x.date == dateList[i]);
                        Level3 thisLevel3Data = new Level3() {
                            id = id,
                            date = dateList[i]
                        };

                        //計算MA (成交量改用萬元作單位)
                        foreach (var d in GlobalSetting.DAYS_BA) {
                            if (lastLevel3 == null) {
                                thisLevel3Data.values[$@"ma_mean_{d}"] = matchedLevel2Data.Nprice_mean;
                                thisLevel3Data.values[$@"ma_volume_{d}"] = matchedLevel1Data.amount / 10000;
                            } else {
                                thisLevel3Data.values[$@"ma_mean_{d}"] =
                                    Ratiolize(lastLevel3.values[$@"ma_mean_{d}"], matchedLevel2Data.Nprice_mean, d - 1, 1);
                                thisLevel3Data.values[$@"ma_volume_{d}"] =
                                    Ratiolize(lastLevel3.values[$@"ma_volume_{d}"], matchedLevel1Data.amount / 10000, d - 1, 1);
                            }
                        }

                        level3DataToInsert.Add(thisLevel3Data);
                        lastLevel3 = thisLevel3Data;
                    }


                    sql.InsertUpdateRow("level3", Level3.GetInsertData(level3DataToInsert));
                    Console.SetCursorPosition(0, currentLineCursor);
                    Console.WriteLine($@"Calculate level 3 id: {id}  ({++count}/{IDList.Count})       ");
                }
            }
        }


        //level4 為進入計算的資料
        public static void InitializeLevel4() {
            using (Sql sql = new Sql()) {
                var newColumns = Level4.column;
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
                foreach (var d in GlobalSetting.DAYS_FP) {
                    newColumns.Add(new SqlColumn($@"future_price_{d}", "decimal(9,2)", false));
                }
                foreach (var d in GlobalSetting.DAYS_FR) {
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

        static decimal Ratiolize(decimal v1, decimal v2, int r1, int r2) {
            return (v1 * r1 + v2 * r2) / (r1 + r2);
        }
    }
}

