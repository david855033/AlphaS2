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

            //**danger
            const bool exportScoreRef = false;
            const bool importScoreRef = true;

            const bool reset7 = false;
            const bool reset56 = false;
            const bool reset234 = false;
            const bool resetLevel1 = false;
            const bool resetFetchLog = false;
            const bool resetAndGenerateScoreRef = false;
            //


            const bool doDownload = false;            //** build env
            const bool generateRoutine = false;       //** build env (level 1-4)

            const bool doLevel56 = false;  //for calculate score


            const bool doLevel7 = false;                  //** build env


            const bool tradeSim = false;
            const bool makeAdvice = false;    //** build env

            FileWriter.CheckDirectory();

            InitializeDTO();

            //StockManager.DropAllList();
            //StockManager.Initialize();

            if (importScoreRef) {
                ScoreManager.DropScoreRef();
                ScoreManager.InitializeScoreRef();
                ScoreManager.ImportScoreRef();
            }
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
                //StockManager.GenerateLevel6();
            }
            if (resetAndGenerateScoreRef) {
                ScoreManager.DropScoreRef();
                ScoreManager.InitializeScoreRef();
                ScoreManager.GenerateScoreTable();
            }

            if (exportScoreRef) {
                ScoreManager.ExportScoreRef();
            }

            if (doLevel7) {
                StockManager.GenerateLevel7();
            }

            if (tradeSim) {
                TradeSimulator.Start();
            }

            if (makeAdvice) {
                Advicer.MakeAdvice();
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
