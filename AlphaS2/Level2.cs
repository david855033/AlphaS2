using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class Level2
    {
        public string id;
        public DateTime date;
        public decimal volume;
        public decimal volume_per_trade;

        private decimal _divide;
        public decimal Divide { get => Math.Round(_divide, 4); set => _divide = value; }

        private decimal _fix;
        public decimal Fix { get => Math.Round(_fix, 4); set => _fix = value; }

        private decimal _changeAbs;
        public decimal ChangeAbs { get => Math.Round(_changeAbs, 2); set => _changeAbs = value; }

        private decimal _price_mean;
        public decimal Price_mean { get => Math.Round(_price_mean, 2); set => _price_mean = value; }

        private decimal _Nprice_mean;
        public decimal Nprice_mean { get => Math.Round(_Nprice_mean, 2); set => _Nprice_mean = value; }

        private decimal _Nprice_open;
        public decimal Nprice_open { get => Math.Round(_Nprice_open, 2); set => _Nprice_open = value; }

        private decimal _Nprice_close;
        public decimal Nprice_close { get => Math.Round(_Nprice_close, 2); set => _Nprice_close = value; }

        private decimal _Nprice_high;
        public decimal Nprice_high { get => Math.Round(_Nprice_high, 2); set => _Nprice_high = value; }

        private decimal _Nprice_low;
        public decimal Nprice_low { get => Math.Round(_Nprice_low, 2); set => _Nprice_low = value; }
        
        public static List<SqlColumn> column =
         new List<SqlColumn> {
                new SqlColumn("id","nchar(10)",false),
                new SqlColumn("date","date",false),
                new SqlColumn("volume","decimal(19,0)",false),
                new SqlColumn("volume_per_trade","decimal(9,2)",false),
                new SqlColumn("divide","decimal(9,4)",false),
                new SqlColumn("fix","decimal(9,4)",false),
                new SqlColumn("change_abs","decimal(9,2)",false),
                new SqlColumn("price_mean","decimal(9,2)",false),
                new SqlColumn("Nprice_mean","decimal(9,2)",false),
                new SqlColumn("Nprice_open","decimal(9,2)",false),
                new SqlColumn("Nprice_close","decimal(9,2)",false),
                new SqlColumn("Nprice_high","decimal(9,2)",false),
                new SqlColumn("Nprice_low","decimal(9,2)",false)
         };



        public static List<Level2> DataAdaptor(DataTable dataTableLevel2) {
            var result = new List<Level2>();
            foreach (DataRow row in dataTableLevel2.Rows) {
                result.Add(new Level2() {
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
                    Nprice_low = (decimal)row["Nprice_low"]
                });
            }
            return result;
        }

        public static SqlInsertData GetInsertData(List<Level2> level2DataToInsert) {
            SqlInsertData result = new SqlInsertData {
                ColumnList = Level2.column,
                primaryKeys = new List<string>() { "id", "date" }
            };
            foreach (var data in level2DataToInsert) {
                result.DataList.Add(new object[] {
                    data.id, data.date,data.volume,data.volume_per_trade, data.Divide, data.Fix, data.ChangeAbs, data.Price_mean,
                 data.Nprice_mean, data.Nprice_open,data.Nprice_close, data.Nprice_high,data.Nprice_low });
            }
            return result;
        }
    }
}
