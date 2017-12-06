using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class FetchLogManager
    {
        public static List<SqlColumn> FETCH_LOG_COLUMN = new List<SqlColumn> {
                    new SqlColumn("type","char(1)",false),
                    new SqlColumn("date","date",false),
                    new SqlColumn("fetch_datetime","smalldatetime",false),
                    new SqlColumn("empty","bit",false),
                    new SqlColumn("uploaded","bit",false)
        };

        public static void InitializeFetchLog() {
            using (Sql sql = new Sql()) {
                DropFetchLog();
                sql.CreateTable("fetch_log", FETCH_LOG_COLUMN);
                sql.SetPrimaryKeys("fetch_log", new string[] { "type", "date" });
            }
        }
        public static List<FetchLog> GetFetchLog() {
            var resultList = new List<FetchLog>();
            using (Sql sql = new Sql()) {
                var dataTable = sql.Select("fetch_log", FETCH_LOG_COLUMN.Select(x => x.name).ToArray());
                foreach (DataRow row in dataTable.Rows) {
                    resultList.Add(new FetchLog() {
                        type = Convert.ToChar(row["type"]),
                        date = (DateTime)row["date"],
                        fetch_datetime = (DateTime)row["fetch_datetime"],
                        empty = (bool)row["empty"],
                        uploaded = (bool)row["uploaded"]
                    });
                }
            }
            return resultList;
        }

        public static List<DateTime> GetDownloadDates(char type) {
            return GetDownloadDates(GetFetchLog(), type);
        }
        public static List<DateTime> GetDownloadDates(List<FetchLog> fetchLog, char type) {
            List<DateTime> resultDateTime = new List<DateTime>();
            for (DateTime currentDate = GlobalSetting.START_DATE;
                currentDate <= GlobalSetting.END_DATE;
                currentDate = currentDate.AddDays(1)) {
                bool fetchLogNotContainCurrentDate = fetchLog.FindIndex(x =>
                    x.date == currentDate &&
                    x.type == type) < 0;
                if (fetchLogNotContainCurrentDate) {
                    resultDateTime.Add(currentDate);
                }
            }
            return resultDateTime;
        }
        static void DropFetchLog() {
            using (Sql sql = new Sql()) {
                sql.DropTable("fetch_log");
            }
        }
    }
}
