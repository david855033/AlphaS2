using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class Column
    {
        public static List<SqlColumn> FETCH_LOG =
            new List<SqlColumn> {
                    new SqlColumn("type","char(1)",false),
                    new SqlColumn("date","date",false),
                    new SqlColumn("fetch_datetime","smalldatetime",false),
                    new SqlColumn("empty","bit",false),
                    new SqlColumn("uploaded","bit",false)
            };

        public static List<SqlColumn> LEVEL1 =
            new List<SqlColumn> {
                new SqlColumn("id","nchar(10)",false),
                new SqlColumn("date","date",false),
                new SqlColumn("deal","decimal(19,0)",false),
                new SqlColumn("amount","decimal(19,0)",false),
                new SqlColumn("price_open","decimal(9,2)",false),
                new SqlColumn("price_close","decimal(9,2)",false),
                new SqlColumn("price_high","decimal(9,2)",false),
                new SqlColumn("price_low","decimal(9,2)",false),
                new SqlColumn("price_ref_nextday","decimal(9,2" +
                    ")",false),
                new SqlColumn("difference","decimal(9,2)",false),
                new SqlColumn("trade","decimal(9,0)",false)
            };
    }
}
