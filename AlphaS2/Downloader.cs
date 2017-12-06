using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class Downloader
    {

        public static string LoadDate(DateTime thisDate, char type = ' ') {
            string url = "";
            if (type == 'A') {
                url = $@"http://www.twse.com.tw/exchangeReport/MI_INDEX?response=csv&date={thisDate.ToString("yyyyMMdd")}&type=ALL";
            } else if (type == 'B') {
                url = $@"http://www.tpex.org.tw/web/stock/aftertrading/otc_quotes_no1430/stk_wn1430_download.php?l=zh-tw&d={thisDate.Year - 1911}/{thisDate.ToString("MM/dd")}&se=AL&s=0,asc,0";
            } else {
                Console.WriteLine("UNDEFINED STOCK TYPE!");
                return "";
            }
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "GET";
            string responseString = "";
            using (WebResponse response = req.GetResponse()) {
                Console.WriteLine($"fetching {url} ...");

                var receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.Default);

                Console.WriteLine("Response stream received.");
                responseString = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }
            bool isEmptyResponse = responseString.Trim() == "";
            using (Sql sql = new Sql()) {
                DataTable selectedFetchLog = sql.Select("fetch_log", new string[] { "type", "date" }, new string[] { $@"type = '{type}'", $@"date = '{thisDate.ToString("yyyy-MM-dd")}'" });
                if (selectedFetchLog.Rows.Count > 0) {
                    sql.UpdateRow("fetch_log",
                        new Dictionary<string, string>() {
                            { "fetch_datetime",$@"'{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}'"},
                            { "empty", isEmptyResponse?"1":"0" },
                            { "uploaded", "0" }
                        },
                        new string[] {
                            $@"type = '{type}'",
                            $@"date = '{thisDate.ToString("yyyy-MM-dd")}'"
                        }
                    );
                    Console.WriteLine($@"new data downloaded, empty={isEmptyResponse}");
                } else {
                    sql.InsertRow("fetch_log", new SqlInsertData() {
                        ColumnList = FetchLogManager.FETCH_LOG_COLUMN,
                        DataList = new List<object[]> {
                        new object[]{ type , thisDate, DateTime.Now, isEmptyResponse, false},
                        }
                    });
                    Console.WriteLine($@"new data uploaded, empty={isEmptyResponse}");
                }
            }
            FileWriter.WriteToFile(thisDate.ToString(type+"_"+"yyyyMMdd"), responseString);
            return responseString;
        }
        public static void LoadDates(List<DateTime> thisDates,char type, int timeOut = 0) {
            Console.WriteLine($@"loading {thisDates.Count} day(s)...");
            int count = 0;
            foreach (var d in thisDates) {
                Console.WriteLine($@"{++count}/{thisDates.Count}");
                LoadDate(d, type);
                if (timeOut > 0) {
                    Thread.Sleep(timeOut);
                }
            }
        }
    }
}
