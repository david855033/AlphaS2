using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class TradeSimulator
    {
        public static void Start() {
            string path = GlobalSetting.TRADE_SIM_PATH;
            Console.WriteLine($@"start Trade Sim @ {path}");
            List<TradeStrategy> strategyList = new List<TradeStrategy>();
        }
    }

    class TradeStrategy {
        public decimal[] WeightVevtor;
        public double BuyThreshold;
        public double SellThreshold;
        public int SellThresholdDay;

        public double BuyPrice;
        public double SellPrice;


    }
}
