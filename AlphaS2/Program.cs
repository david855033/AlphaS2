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
            const bool doDownload = false;
            const bool resetScoreRef = false;
            const bool generateRoutine = false;
            FileWriter.CheckDirectory();

            InitializeDTO();

            //StockManager.DropAllList();
            //StockManager.Initialize();

            if (reset) {
                StockManager.DropLevel6();
                StockManager.DropLevel5(); //
                StockManager.DropLevel4(); //
                StockManager.DropLevel3(); //
                StockManager.DropLevel2(); //
                if (fullReset) {
                    StockManager.DropLevel1(); //
                    StockManager.InitializeLevel1(); //
                }
                StockManager.InitializeLevel2(); //
                StockManager.InitializeLevel3(); //
                StockManager.InitializeLevel4(); //
                StockManager.InitializeLevel5(); //
                StockManager.InitializeLevel6(); //
            }
            StockManager.DropLevel7();
            StockManager.InitializeLevel7();

            if (fullReset) {
                FetchLogManager.InitializeFetchLog(); //
            }

            if (doDownload) {
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
            }

            if (generateRoutine) {  //測試時候省略
                StockManager.GenerateLevel1();
                StockManager.GenerateLevel2();
                StockManager.GenerateLevel3();
                StockManager.GenerateLevel4();
                StockManager.GenerateLevel5();
                StockManager.GenerateLevel6();
            }

            if (resetScoreRef) {
                ScoreManager.DropScoreRef();
                ScoreManager.InitializeScoreRef();
                ScoreManager.GenerateScoreTable();
            }

            StockManager.GenerateLevel7();

            Console.WriteLine("End of Program.");
            Console.ReadKey(false);

        }

        private static void InitializeDTO() {
            Level3.Initiate();
            Level4.Initiate();
            Level5.Initiate();
            Level6.Initiate();
            Level7.Initiate();
            ScoreRef.Initiate();
        }
    }
}
