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
        //fetch log adapter
        public static List<FetchLog> GetFetchLog() {
            return GetFetchLog(new string[] { });
        }
        public static List<FetchLog> GetFetchLog(string[] conditions) {
            var resultList = new List<FetchLog>();
            using (Sql sql = new Sql()) {
                var dataTable = sql.Select("fetch_log", FETCH_LOG_COLUMN.Select(x => x.name).ToArray(),
                    conditions);
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
        public static List<FetchLog> GetFileListToUpload() {
            var result = GetFetchLog(new[] { "uploaded = 0", "empty = 0" });
            Console.WriteLine($@"get {result.Count} file(s) need to upload to level 1...");
            return result;
        }

        //傳回START DATE 與 END DATE內沒有資料的日期
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
