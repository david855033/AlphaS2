﻿using System;
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
            const bool reset7 = true;
            const bool reset56 = true;
            const bool reset234 = true;
            const bool resetLevel1 = true;

            const bool resetFetchLog = true;

            const bool doDownload = true;
            const bool generateRoutine = true;
            const bool doLevel56 = true;

            const bool resetScoreRef = true;

            const bool doLevel7 = true;

            const bool tradeSim = true;

            FileWriter.CheckDirectory();

            InitializeDTO();

            //StockManager.DropAllList();
            //StockManager.Initialize();
            if (reset7) {
                StockManager.DropLevel7();
                if (reset56) {
                    StockManager.DropLevel6();
                    StockManager.DropLevel5(); //
                    if (reset234) {
                        StockManager.DropLevel4(); //
                        StockManager.DropLevel3(); //
                        StockManager.DropLevel2(); //
                        if (resetLevel1) {
                            StockManager.DropLevel1(); //
                            StockManager.InitializeLevel1(); //
                        }
                        StockManager.InitializeLevel2(); //
                        StockManager.InitializeLevel3(); //
                        StockManager.InitializeLevel4(); //
                    }
                    StockManager.InitializeLevel5(); //
                    StockManager.InitializeLevel6(); //
                }
                StockManager.InitializeLevel7();
            }

            if (resetFetchLog) {
                FetchLogManager.InitializeFetchLog(); //
            }

            if (doDownload) {
                List<DateTime> downloadDatesA = FetchLogManager.GetDownloadDates('A'); //上市
                List<DateTime> downloadDatesB = FetchLogManager.GetDownloadDates('B'); //上櫃
                List<DateTime> downloadDatesZ = FetchLogManager.GetDownloadDates('Z'); //上櫃除權息
                Downloader.LoadDates(downloadDatesA, 'A', 4000);
                Downloader.LoadDates(downloadDatesB, 'B', 3000);
                Downloader.LoadDates(downloadDatesZ, 'Z', 5000);
            }

            if (generateRoutine) {
                StockManager.GenerateLevel1();
                StockManager.GenerateLevel2();
                StockManager.GenerateLevel3();
                StockManager.GenerateLevel4();
            }
            if (doLevel56) {
                StockManager.GenerateLevel5();
                StockManager.GenerateLevel6();
            }
            if (resetScoreRef) {
                ScoreManager.DropScoreRef();
                ScoreManager.InitializeScoreRef();
                ScoreManager.GenerateScoreTable();
            }

            if (doLevel7) {
                StockManager.GenerateLevel7();
            }

            if (tradeSim) {
                TradeSimulator.Start();
            }

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
