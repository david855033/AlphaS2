using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
namespace AlphaS2
{
    class Program
    {
        static readonly DateTime START_DATE = new DateTime(2005, 1, 3);

        static void Main(string[] args) {
            using (Sql sql = new Sql()) {
                //StockManager.DropAllList();
                //StockManager.Initialize();

                FileWriter.CheckDirectory();
                var thisDate = new DateTime(2005, 1, 3);
                var response = Downloader.LoadDate(thisDate);
                FileWriter.WriteToFile(thisDate.ToString("yyyyMMdd"),response);

                
                Console.ReadKey(false);
            }
        }
    }
}
