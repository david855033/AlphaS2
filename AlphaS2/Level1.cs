using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class Level1 {
        public string id;
        public DateTime date;
        public decimal deal;
        public decimal amount;
        public decimal price_open;
        public decimal price_close;
        public decimal price_close_nonzero;
        public decimal price_high;
        public decimal price_low;
        public decimal price_ref_nextday;
        public decimal difference;
        public decimal trade;

        public static List<SqlColumn> column =
          new List<SqlColumn> {
                new SqlColumn("id","nchar(10)",false),
                new SqlColumn("date","date",false),
                new SqlColumn("deal","decimal(19,0)",false),
                new SqlColumn("amount","decimal(19,0)",false),
                new SqlColumn("price_open","decimal(9,2)",false),
                new SqlColumn("price_close","decimal(9,2)",false),
                new SqlColumn("price_close_nonzero","decimal(9,2)",false),
                new SqlColumn("price_high","decimal(9,2)",false),
                new SqlColumn("price_low","decimal(9,2)",false),
                new SqlColumn("price_ref_nextday","decimal(9,2)",false),
                new SqlColumn("difference","decimal(9,2)",false),
                new SqlColumn("trade","decimal(9,0)",false)
          };

        public static List<Level1> DataAdaptor(DataTable dataTableLevel1) {
            var result = new List<Level1>();
            foreach (DataRow row in dataTableLevel1.Rows) {
                result.Add(new Level1() {
                    id = ((string)row["id"]).Trim(),
                    date = (DateTime)row["date"],
                    deal = (decimal)row["deal"],
                    amount = (decimal)row["amount"],
                    price_open = (decimal)row["price_open"],
                    price_close = (decimal)row["price_close"],
                    price_close_nonzero = (decimal)row["price_close"],
                    price_high = (decimal)row["price_high"],
                    price_low = (decimal)row["price_low"],
                    price_ref_nextday = (decimal)row["price_ref_nextday"],
                    difference = (decimal)row["difference"],
                    trade = (decimal)row["trade"]
                });
            }
            return result;
        }
    }
}

