using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class Level2JoinLevel3 : Level2
    {
        private decimal _min_volume_60;
        public decimal Min_volume_60 { get => Math.Round(_min_volume_60, 2); set => _min_volume_60 = value; }

        private decimal _max_change_abs_120;
        public decimal Max_change_abs_120 { get => Math.Round(_max_change_abs_120, 2); set => _max_change_abs_120 = value; }


        public new static List<SqlColumn> column =
          new List<SqlColumn> {
                    new SqlColumn("id","nchar(10)",false),
                    new SqlColumn("date","date",false),
                    new SqlColumn("volume","decimal(19,0)",false),
                    new SqlColumn("volume_per_trade","decimal(9,2)",false),
                    new SqlColumn("divide","decimal(9,4)",false),
                    new SqlColumn("fix","decimal(9,4)",false),
                    new SqlColumn("change_abs","decimal(9,4)",false),
                    new SqlColumn("price_mean","decimal(9,2)",false),
                    new SqlColumn("Nprice_mean","decimal(9,2)",false),
                    new SqlColumn("Nprice_open","decimal(9,2)",false),
                    new SqlColumn("Nprice_close","decimal(9,2)",false),
                    new SqlColumn("Nprice_high","decimal(9,2)",false),
                    new SqlColumn("Nprice_low","decimal(9,2)",false),
                    new SqlColumn("min_volume_60","decimal(19,0)",false),
                    new SqlColumn("max_change_abs_120","decimal(9,4)",false)
          };



        public new static List<Level2JoinLevel3> DataAdaptor(DataTable dataTableLevel2) {
            var result = new List<Level2JoinLevel3>();
            foreach (DataRow row in dataTableLevel2.Rows) {
                result.Add(new Level2JoinLevel3() {
                    id = ((string)row["id"]).Trim(),
                    date = (DateTime)row["date"],
                    volume = (decimal)row["volume"],    //單位為萬
                    volume_per_trade = (decimal)row["volume_per_trade"],
                    Divide = (decimal)row["divide"],
                    Fix = (decimal)row["fix"],
                    ChangeAbs = (decimal)row["change_abs"],
                    Price_mean = (decimal)row["price_mean"],
                    Nprice_mean = (decimal)row["Nprice_mean"],
                    Nprice_open = (decimal)row["Nprice_open"],
                    Nprice_close = (decimal)row["Nprice_close"],
                    Nprice_high = (decimal)row["Nprice_high"],
                    Nprice_low = (decimal)row["Nprice_low"],
                    Min_volume_60 = (decimal)row["min_volume_60"],
                    Max_change_abs_120 = (decimal)row["max_change_abs_120"]
                });
            }
            return result;
        }

        public static SqlInsertData GetInsertData(List<Level2JoinLevel3> level2DataToInsert) {
            SqlInsertData result = new SqlInsertData();
            result.ColumnList = Level2JoinLevel3.column;
            result.primaryKeys = new List<string>() { "id", "date" };
            foreach (var data in level2DataToInsert) {
                result.DataList.Add(new object[] {
                    data.id, data.date,data.volume,data.volume_per_trade, data.Divide, data.Fix, data.ChangeAbs, data.Price_mean,
                 data.Nprice_mean, data.Nprice_open,data.Nprice_close, data.Nprice_high,data.Nprice_low,data.Min_volume_60,data.Max_change_abs_120 });
            }
            return result;
        }
    }
}
