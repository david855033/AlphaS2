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
            const bool reset = false;
            const bool fullReset = false;
            FileWriter.CheckDirectory();

            initializeDTO();

            //StockManager.DropAllList();
            //StockManager.Initialize();

            if (reset) {
                StockManager.DropLevel6();
                StockManager.DropLevel5(); //
                StockManager.DropLevel4(); //
                StockManager.DropLevel3(); //
                if (fullReset) {
                    StockManager.DropLevel2(); //
                    StockManager.DropLevel1(); //
                    StockManager.InitializeLevel1(); //
                    StockManager.InitializeLevel2(); //
                }
                StockManager.InitializeLevel3(); //
                StockManager.InitializeLevel4(); //
                StockManager.InitializeLevel5(); //
                StockManager.InitializeLevel6();
            }

            if (fullReset) {
                FetchLogManager.InitializeFetchLog(); //
            }

            ScoreManager.DropScoreRef();
            ScoreManager.InitializeScoreRef();

            List<DateTime> downloadDatesA = FetchLogManager.GetDownloadDates('A');
            List<DateTime> downloadDatesB = FetchLogManager.GetDownloadDates('B');
            List<DateTime> downloadDatesZ = FetchLogManager.GetDownloadDates('Z');
            Task.WaitAll(new[] {
                    Task.Factory.StartNew(() =>  {
                        Downloader.LoadDates(downloadDatesA, 'A', 2000);
                        Downloader.LoadDates(downloadDatesZ, 'Z', 3000);
                    }),
                    Task.Factory.StartNew(() =>  Downloader.LoadDates(downloadDatesB, 'B', 2000))
                });

            if (false) {  //測試時候省略
                StockManager.GenerateLevel1(); 
                StockManager.GenerateLevel2(); 
                StockManager.GenerateLevel3(); 
                StockManager.GenerateLevel4(); 
                StockManager.GenerateLevel5(); 
                StockManager.GenerateLevel6();
            }



            Console.WriteLine("End of Program.");
            Console.ReadKey(false);

        }

        private static void initializeDTO() {
            Level3.Initiate();
            Level4.Initiate();
            Level5.Initiate();
            Level6.Initiate();
            ScoreRef.Initiate();
        }
    }
}
