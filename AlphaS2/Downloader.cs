using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class Downloader
    {
        public static string LoadDate(DateTime thisDate) {
            var url = $@"http://www.twse.com.tw/exchangeReport/MI_INDEX?response=csv&date={thisDate.ToString("yyyyMMdd")}&type=ALL";

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "GET";
            string responseString = "";
            using (WebResponse response = req.GetResponse()) {
                Console.WriteLine($"fetching {url} ...");

                var receiveStream= response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.Default);

                Console.WriteLine("Response stream received.");
                responseString = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }
            return responseString;
        }
    }
}
