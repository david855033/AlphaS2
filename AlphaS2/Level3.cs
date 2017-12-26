using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class Level3
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
            //mean average
            foreach (var c in new string[] { "mean", "volume" }) {
                foreach (var d in GlobalSetting.DAYS_BA) {
                    newColumns.Add(new SqlColumn($@"ma_{c}_{d}", "decimal(9,2)", false));
                }
            }
            //MACD-dif: 不同條moving average間之差異 使用mean
            //MACD-dem: dif之10日ma (在level3)
            //MACD: dif-dem  (在level4)
            foreach (var d1 in GlobalSetting.DAYS_MACD) {
                foreach (var d2 in GlobalSetting.DAYS_MACD.Where(x => x > d1)) {
                    newColumns.Add(new SqlColumn($@"dif_{d1}_{d2}", "decimal(9,2)", false));
                    newColumns.Add(new SqlColumn($@"dem_{d1}_{d2}", "decimal(9,2)", false));
                }
            }
            //max min極大極小值 for KDJ
            foreach (var c in new string[] { "price" }) {
                foreach (var d in GlobalSetting.DAYS_KD) {
                    newColumns.Add(new SqlColumn($@"max_{c}_{d}", "decimal(9,2)", false));
                    newColumns.Add(new SqlColumn($@"min_{c}_{d}", "decimal(9,2)", false));
                }
            }
            //DMI
            //posdm = 今日最高-昨日最高(只取正值)
            //negdm = 昨日最低-今日最低(只取正值)
            //tr = max(H - L, H-C(t-1), L-C(t-1))
            foreach (var c in new string[] { "posdm", "negdm", "tr" }) {
                foreach (var d in GlobalSetting.DAYS_DMI) {
                    newColumns.Add(new SqlColumn($@"{c}_{d}", "decimal(9,2)", false));
                }
            }
            Level3.column = newColumns;
        }
        public static List<Level3> DataAdaptor(DataTable dataTableLevel2) {
            var result = new List<Level3>();
            foreach (DataRow row in dataTableLevel2.Rows) {
                var newLevel3 = new Level3() {
                    id = ((string)row["id"]).Trim(),
                    date = (DateTime)row["date"]
                };
                foreach (string c in column.Select(x => x.name)) {
                    newLevel3.values[c] = (decimal)row[c];
                }
                result.Add(newLevel3);
            }
            return result;
        }

        public static SqlInsertData GetInsertData(List<Level3> level3DataToInsert) {
            SqlInsertData result = new SqlInsertData {
                ColumnList = Level3.column,
                primaryKeys = new List<string>() { "id", "date" }
            };
            foreach (var data in level3DataToInsert) {
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
