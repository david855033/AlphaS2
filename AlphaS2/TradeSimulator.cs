using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AlphaS2
{
    class TradeSimulator
    {
        static List<TradeStrategy> strategyList;
        static List<DayData> dateDataList = new List<DayData>();
        public static void Start() {
            GenerateStrategy();
            InitializeDayData(GlobalSetting.START_SIM_DATE, GlobalSetting.END_SIM_DATE);
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
        private static void InitializeDayData(DateTime start, DateTime end) {
            List<DateTime> days = new List<DateTime>();
            for (DateTime i = start; i <= end && i <= GlobalSetting.END_DATE; i = i.AddDays(1)) {
                days.Add(i);
            }
            using (var sql = new Sql()) {
                int position = Console.CursorTop;
                //load data
                foreach (var date in days) {
                    Console.CursorTop = position;
                    Console.WriteLine($@"doing {date}");
                    DataTable dataTable = sql.Select("level7 join level2 ",
                        new string[] { "level2.id as id", "Nprice_open", "Nprice_close", "Nprice_high", "Nprice_low" }
                        .Concat(GlobalSetting.DAYS_FP.Select(x => $"future_price_{x}"))
                        .Concat(GlobalSetting.DAYS_FR.Select(x => $"future_rank_{x}")),
                        $@"on level7.id = level2.id
                        and level7.date = level2.date 
                        where level2.date = '{date.ToString("yyyy-MM-dd")}' order by level2.id"
                        );
                    if (dataTable.Rows.Count == 0) { continue; };
                    var dayData = new DayData() { date = date };
                    foreach (DataRow row in dataTable.Rows) {
                        dayData.StockData.Add(
                            new StockData {
                                id = (string)row["id"],
                                nprice_open = (decimal)row["Nprice_open"],
                                nprice_close = (decimal)row["Nprice_close"],
                                nprice_high = (decimal)row["Nprice_high"],
                                nprice_low = (decimal)row["Nprice_low"],
                                ScoreVector = GlobalSetting.DAYS_FP.Select(x => (decimal)row[$"future_price_{x}"])
                                .Concat(GlobalSetting.DAYS_FR.Select(x => (decimal)row[$"future_rank_{x}"]))
                                .ToArray()
                            }
                        );
                    }
                    dateDataList.Add(dayData);
                }
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


        class TradeStrategy
        {
            public decimal[] WeightVevtor;
            public float BuyThreshold;
            public float SellThreshold;
            public float SellThresholdDay;
            public float SetBuyPrice;
            public float SetSellPrice;
        }

        class DayData
        {
            public DateTime date;
            public List<StockData> StockData = new List<StockData>();
        }

        class StockData
        {
            public string id;
            public decimal nprice_open, nprice_close, nprice_high, nprice_low;
            public decimal[] ScoreVector;
            public override string ToString() {
                return id + " " + $@"Price{nprice_open}/{nprice_close}/{nprice_high}/{nprice_low}";
            }

            class StockInformation
            {
                public string id;
                public decimal StartDateInPeroid;
                public decimal EndDateInPeroid;
            }

            public class TradeStrategy
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
    }
}
