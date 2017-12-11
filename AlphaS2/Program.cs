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


        static void Main(string[] args) {
            using (Sql sql = new Sql()) {
                
                FileWriter.CheckDirectory();

                FetchLogManager.InitializeFetchLog();

                List<DateTime> downloadDatesA = FetchLogManager.GetDownloadDates('A');
                List<DateTime> downloadDatesB = FetchLogManager.GetDownloadDates('B');
                Task.WaitAll(new[] {
                    Task.Factory.StartNew(() =>  Downloader.LoadDates(downloadDatesA, 'A', 2000)),
                    Task.Factory.StartNew(() =>  Downloader.LoadDates(downloadDatesA, 'B', 2000))
                });


                StockManager.DropAllList();
                StockManager.Initialize();

                List<FetchLog> FileListToUpload= FetchLogManager.GetFileListToUpload();
                StockManager.GenerateLevel1(FileListToUpload);
                Console.ReadKey(false);
            }
        }
    }
}
