using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class TradeSimulator {
        public static void Start() {
            string path = GlobalSetting.TRADE_SIM_PATH;
            Console.WriteLine($@"start Trade Sim @ {path}");
            List<TradeStrategy> strategyList = new List<TradeStrategy>();
            for (int i = 0; i < strategyList.Count(); i++) {
                string fileName = $@"simulation {i.ToString("D6")}.txt";
            }
        }
    }

    class DayData {
        public DateTime date;
        public List<StockData> StockData;
    }
    class StockData {
        public string id;
        public decimal nprice_open, nprice_close, nprice_high, nprice_low;
        public decimal[] ScoreVector;
    }
    class StockInformation {
        public string id;
        public decimal StartDateInPeroid;
        public decimal EndDateInPeroid;
    }

    class TradeStrategy {
        public decimal[] WeightVevtor;
        public double BuyThreshold;
        public double SellThreshold;
        public int SellThresholdDay;

        public double SetBuyPrice;
        public double SetSellPrice;
    }

    class TradeRecord {
        public DateTime BuyDate;
        public DateTime SellDate;
        public double BuyPrice;
        public double SellPrice;
    }

    class TradeSimulation {
        public DateTime StartSimulationDate;
        public DateTime EndSimulationDate;
        public TradeStrategy TradeStrategy ;
        public List<TradeRecord> HoldingStocks;
        public List<TradeRecord> FinishedStocks;
        public double fund;
    }
}
