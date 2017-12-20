﻿using System;
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

        public static string LoadDate(DateTime thisDate, char type = ' ', int timeOut = 0) {
            string url = "";
            string filePath = GlobalSetting.FOLDER_PATH + "\\" + type + "_" + thisDate.ToString("yyyyMMdd") + ".txt";
            bool fileExist = File.Exists(filePath);

            if (type == 'A') {
                url = $@"http://www.twse.com.tw/exchangeReport/MI_INDEX?response=csv&date={thisDate.ToString("yyyyMMdd")}&type=ALL";
            } else if (type == 'B') {
                url = $@"http://www.tpex.org.tw/web/stock/aftertrading/daily_close_quotes/stk_quote_download.php?l=zh-tw&d={thisDate.Year - 1911}/{thisDate.ToString("MM/dd")}&s=0,asc,0";
            } else if (type == 'Z') {
                url = $@"http://www.twse.com.tw/exchangeReport/TWT49U?response=csv&strDate={thisDate.ToString("yyyyMMdd")}&endDate={thisDate.ToString("yyyyMMdd")}";
            } else {
                Console.WriteLine("UNDEFINED STOCK TYPE!");
                return "";
            }
            string responseString = "";
            if (fileExist) {
                using (StreamReader sr = new StreamReader(filePath, Encoding.Default)) {
                    responseString = sr.ReadToEnd();
                    Console.WriteLine($@"existed file: {filePath}");
                }
            } else {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "GET";
                using (WebResponse response = req.GetResponse()) {
                    Console.WriteLine($"fetching {url} ...");

                    var receiveStream = response.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.Default);

                    Console.WriteLine("Response stream received.");
                    responseString = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();
                }
                if (timeOut > 0) {
                    Thread.Sleep(timeOut);
                }
                FileWriter.WriteToFile(thisDate.ToString(type + "_" + "yyyyMMdd"), responseString);
            }

            bool isEmptyResponse = responseString.Trim() == "" || responseString.IndexOf("\"上櫃家數\",\"0\"") >= 0;
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
                    sql.InsertUpdateRow("fetch_log", new SqlInsertData() {
                        ColumnList = FetchLog.column,
                        DataList = new List<object[]> {
                        new object[]{ type , thisDate, DateTime.Now, isEmptyResponse, false},
                        }
                    });
                    Console.WriteLine($@"new data uploaded, empty={isEmptyResponse}");
                }
            }
            return responseString;
        }
        public static void LoadDates(List<DateTime> thisDates, char type, int timeOut = 0) {
            Console.WriteLine($@"loading {thisDates.Count} day(s)...");
            int count = 0;
            foreach (var d in thisDates) {
                Console.WriteLine($@"{++count}/{thisDates.Count}");
                LoadDate(d, type, timeOut);

            }
        }
    }
}
