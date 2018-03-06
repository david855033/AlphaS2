using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class TradeSimulator
    {
        static List<TradeStrategy> strategyList;
        static List<DayData> dayData;
        public static void Start() {
            GenerateStrategy();
            InitializeDayData();
            string path = GlobalSetting.TRADE_SIM_PATH;
            FileWriter.CheckDirectory();
            Console.WriteLine($@"start Trade Sim @ {path}");

            for (int i = 0; i < strategyList.Count(); i++) {
                string fileName = $@"simulation {i.ToString("D6")}.txt";
                var currentStrategy = strategyList[i];
                Console.WriteLine($@">> simulating {fileName}");
                Console.WriteLine($@">> detail: {currentStrategy.ToString()}");
            }
        }

        private static void InitializeDayData() {
            using (var sql = new Sql()) {

            }
        }

        private static void GenerateStrategy() {
            strategyList = new List<TradeStrategy>();
            strategyList.Add(new TradeStrategy {
                WeightVevtor = new decimal[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                BuyThreshold = 100,
                SellThreshold = 100,
                SellThresholdDay = 5,
                SetBuyPrice = 1,
                SetSellPrice = 1
            });
        }
    }

    class DayData
    {
        public DateTime date;
        public List<StockData> StockData;
    }
    class StockData
    {
        public string id;
        public decimal nprice_open, nprice_close, nprice_high, nprice_low;
        public decimal[] ScoreVector;
    }
    class StockInformation
    {
        public string id;
        public decimal StartDateInPeroid;
        public decimal EndDateInPeroid;
    }

    class TradeStrategy
    {
        public decimal[] WeightVevtor;
        public double BuyThreshold;
        public double SellThreshold;
        public int SellThresholdDay;
        
        public double SetBuyPrice;
        public double SetSellPrice;
        public override string ToString() {
            return $@"WeightVevtor: {string.Join(",", this.WeightVevtor)}, B-S Threshold: {BuyThreshold}/{SellThreshold}, Sell Day {SellThresholdDay}, B-S Price: {SetBuyPrice}/{SetSellPrice}"; 
        }
    }

    class TradeRecord
    {
        public DateTime BuyDate;
        public DateTime SellDate;
        public double BuyPrice;
        public double SellPrice;
    }

    class TradeSimulation
    {
        public DateTime StartSimulationDate;
        public DateTime EndSimulationDate;
        public TradeStrategy TradeStrategy;
        public List<TradeRecord> HoldingStocks;
        public List<TradeRecord> FinishedStocks;
        public double fund;
    }
}
