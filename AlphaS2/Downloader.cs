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
        static DateTime startdate = new DateTime(2005, 1, 1);

        public static void LoadDate(DateTime thisDate) {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create($@"http://www.twse.com.tw/exchangeReport/MI_INDEX?response=csv&date=20040401&type=ALL");
            req.Method = "GET";
            using (WebResponse response = req.GetResponse()) {

                var receiveStream= response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.Default);

                Console.WriteLine("Response stream received.");
                Console.WriteLine(readStream.ReadToEnd());
                response.Close();
                readStream.Close();
            }
        }
    }
}
