using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class FetchLog
    {
        public char type;
        public DateTime date;
        public DateTime fetch_datetime;
        public bool empty;
        public bool uploaded;
        public string FilePath {
            get {
                return AlphaS2.GlobalSetting.FOLDER_PATH + $@"\{type}_{date.ToString("yyyyMMdd")}.txt";
            }
        }
        public static List<SqlColumn> column =
            new List<SqlColumn> {
                    new SqlColumn("type","char(1)",false),
                    new SqlColumn("date","date",false),
                    new SqlColumn("fetch_datetime","smalldatetime",false),
                    new SqlColumn("empty","bit",false),
                    new SqlColumn("uploaded","bit",false)
            };
    }
}
