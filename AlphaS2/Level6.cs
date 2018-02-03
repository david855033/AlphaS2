using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AlphaS2
{
    class Level6
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

            foreach (var d in GlobalSetting.DAYS_FR) {
                newColumns.Add(new SqlColumn($@"future_rank_{d}", "decimal(9,2)", false));
            }
            Level6.column = newColumns;
        }
        public static List<Level6> DataAdaptor(DataTable dataTableLevel6) {
            var result = new List<Level6>();
            foreach (DataRow row in dataTableLevel6.Rows) {
                var newLevel6 = new Level6() {
                    id = ((string)row["id"]).Trim(),
                    date = (DateTime)row["date"]
                };
                foreach (string c in column.Select(x => x.name).Where(x => x != "id" && x != "date")) {
                    newLevel6.values[c] = (decimal)row[c];
                }
                result.Add(newLevel6);
            }
            return result;
        }

        public static SqlInsertData GetInsertData(List<Level6> level6DataToInsert) {
            SqlInsertData result = new SqlInsertData {
                ColumnList = Level6.column,
                primaryKeys = new List<string>() { "id", "date" }
            };
            foreach (var data in level6DataToInsert) {
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
