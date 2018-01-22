using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class Level4
    {
        public string id;
        public DateTime date;
        public Dictionary<string, decimal> values = new Dictionary<string, decimal>();

        public static List<SqlColumn> column;

        public static void Initiate() {
            var newColumns = new List<SqlColumn>() {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("date","date",false)
                };
            //BA 當日與指定時間與moving average的差距，換算成log10
            foreach (var c in new string[] { "mean", "volume" }) {
                foreach (var d in GlobalSetting.DAYS_BA) {
                    newColumns.Add(new SqlColumn($@"ba_{c}_{d}", "decimal(9,2)", false));
                }
            }

            //dif_cost_mean(一段時間的交易成本cost與每日平均交易價mean的差距)，換算成log10
            foreach (var d in GlobalSetting.DAYS_BA) {
                newColumns.Add(new SqlColumn($@"dif_cost_mean{d}", "decimal(9,2)", false));
            }

            //MACD-dif: 不同條moving average間之差異 使用mean
            //MACD-dem: dif之10日ma (僅在level3)
            //MACD: dif-dem  (僅在level4)
            foreach (var d1 in GlobalSetting.DAYS_MACD) {
                foreach (var d2 in GlobalSetting.DAYS_MACD.Where(x => x > d1)) {
                    newColumns.Add(new SqlColumn($@"macd_{d1}_{d2}", "decimal(9,2)", false));
                }
            }

            //RSV KD (平滑率=1/3), J=K-D
            foreach (var c in new string[] { "rsv", "k", "d", "j" }) {
                foreach (var d in GlobalSetting.DAYS_KD) {
                    newColumns.Add(new SqlColumn($@"{c}_{d}", "decimal(9,2)", false));
                }
            }

            //RSI
            foreach (var d in GlobalSetting.DAYS_RSI) {
                newColumns.Add(new SqlColumn($@"rsi_{d}", "decimal(9,2)", false));
            }
            foreach (var d1 in GlobalSetting.DAYS_RSI) {
                foreach (var d2 in GlobalSetting.DAYS_RSI.Where(x => x > d1)) {
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
                foreach (var d in GlobalSetting.DAYS_DMI) {
                    newColumns.Add(new SqlColumn($@"{c}_{d}", "decimal(9,2)", false));
                }
            }
            //W%R = Hn-Cn / Hn - Ln
            foreach (var c in new string[] { "wr" }) {
                foreach (var d in GlobalSetting.DAYS_KD) {
                    newColumns.Add(new SqlColumn($@"{c}_{d}", "decimal(9,2)", false));
                }
            }
            Level4.column = newColumns;
        }
        public static List<Level4> DataAdaptor(DataTable dataTableLevel4) {
            var result = new List<Level4>();
            foreach (DataRow row in dataTableLevel4.Rows) {
                var newLevel4 = new Level4() {
                    id = ((string)row["id"]).Trim(),
                    date = (DateTime)row["date"]
                };
                foreach (string c in column.Select(x => x.name).Where(x => x != "id" && x != "date")) {
                    newLevel4.values[c] = (decimal)row[c];
                }
                result.Add(newLevel4);
            }
            return result;
        }

        public static SqlInsertData GetInsertData(List<Level4> level4DataToInsert) {
            SqlInsertData result = new SqlInsertData {
                ColumnList = Level4.column,
                primaryKeys = new List<string>() { "id", "date" }
            };
            foreach (var data in level4DataToInsert) {
                var newObjects = new List<object>() {
                    data.id, data.date
                };
                foreach (string c in column.Select(x => x.name)) {
                    if (c == "id" || c == "date") { continue; }
                    if (data.values.TryGetValue(c, out decimal v)) {
                        newObjects.Add(Math.Round(v, 2));
                    } else {
                        newObjects.Add(-1000);
                    }
                }
                result.DataList.Add(newObjects.ToArray());
            }
            return result;
        }
    }


}

