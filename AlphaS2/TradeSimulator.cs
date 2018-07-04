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
        static List<DayData> serialDayData = new List<DayData>();
        static Dictionary<string, StockInformation> stockInformations = new Dictionary<string, StockInformation>();

        public static void Start() {
            GenerateStrategy();
            InitializeDayData(GlobalSetting.START_SIM_DATE, GlobalSetting.END_SIM_DATE);
            string path = GlobalSetting.TRADE_SIM_PATH;
            Console.WriteLine($@"start Trade Sim @ {path}");
            const bool DEBUG = true;


            for (int i = 0; i < strategyList.Count(); i++) {
                string fileName = $@"simulation {i.ToString("D6")}.txt";
                var strategy = strategyList[i];
                Console.WriteLine($@">> simulating {fileName}");
                Console.WriteLine($@">> detail: {strategy.ToString()}");
                double fund = 1;
                var simulation = new TradeSimulation(strategy, serialDayData.First().date, serialDayData.Last().date);

                if (DEBUG) Console.WriteLine("策略編號#" + i);
                //初始化下單之清單(日與日之間傳遞)
                List<BuyOrder> buyOrderList = new List<BuyOrder>();
                List<SellOrder> sellOrderList = new List<SellOrder>();
                for (int d = 0; d < serialDayData.Count(); d++) {
                    //計算權重
                    var currentDay = serialDayData[d];
                    if (DEBUG) Console.WriteLine("\r\n>>>>>>>>日期" + currentDay.date.ToShortDateString() + " 資金:"+fund);
                    foreach (var stock in currentDay.StockData) {
                        double weightScore = 0;
                        for (int j = 0; j < stock.ScoreVector.Length; j++) {
                            weightScore += (double)stock.ScoreVector[j] * (double)strategy.WeightVevtor[j];
                        }
                        stock.WeightedScore = weightScore;
                    }

                    //判斷昨日下單是否成功交易
                    if (DEBUG) Console.WriteLine("###判斷昨日下單是否成功交易###");
                    if (DEBUG) Console.WriteLine("今日已掛--買進:" + buyOrderList.Count() + "賣出:" + sellOrderList.Count());
                    //處理買單
                    var invest = fund / (simulation.TradeStrategy.MaxDivide - simulation.HoldingStocks.Count());
                    foreach (var buyOrder in buyOrderList) {
                        if (DEBUG) Console.WriteLine("ID:" + buyOrder.stock.id + " 掛買:" + buyOrder.setBuyPrice.ToString("F2"));
                        var matchStock = currentDay.StockData.FirstOrDefault(x => x.id == buyOrder.stock.id);
                        if (matchStock != null) {
                            if (DEBUG) Console.WriteLine("- 當日開盤:" + matchStock.nprice_open + " 最低:" + matchStock.nprice_low);
                            if (buyOrder.setBuyPrice >= (double)matchStock.nprice_open) {
                                if (DEBUG) Console.WriteLine("- 開盤價買進, 投入" + invest.ToString("F4"));
                                simulation.HoldingStocks.Add(
                                    new TradeRecord() {
                                        stock = matchStock,
                                        BuyDate = currentDay.date,
                                        BuyPrice = (double)matchStock.nprice_open,
                                        invest = invest
                                    });
                                fund -= invest;
                            } else if (buyOrder.setBuyPrice >= (double)matchStock.nprice_low) {
                                if (DEBUG) Console.WriteLine("- 設定價格買進, 投入" + invest.ToString("F4"));
                                simulation.HoldingStocks.Add(
                                   new TradeRecord() {
                                       stock = matchStock,
                                       BuyDate = currentDay.date,
                                       BuyPrice = buyOrder.setBuyPrice,
                                       invest = invest
                                   });
                                fund -= invest;
                            } else {
                                if (DEBUG) Console.WriteLine("- 無成交");
                            }
                        } else {
                            if (DEBUG) Console.WriteLine("- 搜尋不到該股");
                        }
                    }
                    buyOrderList.Clear();

                    //處理賣單
                    foreach (var sellOrder in sellOrderList) {
                        if (DEBUG) Console.WriteLine("ID:" + sellOrder.stock.id + " 掛賣:" + sellOrder.setSellPrice.ToString("F2"));
                        var matchStock = currentDay.StockData.FirstOrDefault(x => x.id == sellOrder.stock.id);
                        if (matchStock != null) {
                            if (DEBUG) Console.WriteLine("- 當日開盤:" + matchStock.nprice_open + " 最高:" + matchStock.nprice_high);
                            if (sellOrder.setSellPrice <= (double)matchStock.nprice_open) {
                                var sellPrice = (double)matchStock.nprice_open;
                                var tradeRecord = simulation.HoldingStocks.Find(x => x.stock.id == matchStock.id);
                                var getFund = tradeRecord.getFund(sellPrice);
                                if (DEBUG) Console.WriteLine("- 開盤價賣出, 回收資金" + getFund.ToString("F4"));
                                fund += getFund;
                                tradeRecord.SellDate = currentDay.date;
                                tradeRecord.SellPrice = sellPrice;
                                simulation.FinishedStocks.Add(tradeRecord);
                                simulation.HoldingStocks.Remove(tradeRecord);
                            } else if (sellOrder.setSellPrice <= (double)matchStock.nprice_high) {
                                var sellPrice = sellOrder.setSellPrice;
                                var tradeRecord = simulation.HoldingStocks.Find(x => x.stock.id == matchStock.id);
                                var getFund = tradeRecord.getFund(sellPrice);
                                if (DEBUG) Console.WriteLine("- 設定價格賣出, 回收資金" + getFund.ToString("F4"));
                                fund += getFund;
                                tradeRecord.SellDate = currentDay.date;
                                tradeRecord.SellPrice = sellPrice;
                                simulation.FinishedStocks.Add(tradeRecord);
                                simulation.HoldingStocks.Remove(tradeRecord);
                            } else {
                                if (DEBUG) Console.WriteLine("- 無成交");
                            }
                        } else {
                            //todo找不到->強制結算
                        }
                    }
                    sellOrderList.Clear();

                    //若還有空間，排序可購買清單
                    if (DEBUG) Console.WriteLine("###判斷是否要掛明日買單###");
                    if (simulation.HoldingStocks.Count() < strategy.MaxDivide) {
                        var ToBuy = currentDay.StockData.Where(x => x.WeightedScore >= strategy.BuyThreshold).ToList();
                        ToBuy.Sort((a, b) => b.WeightedScore.CompareTo(a.WeightedScore));

                        var BuyCount = Math.Min(strategy.MaxBuyInDay, strategy.MaxDivide - simulation.HoldingStocks.Count());
                        for (int j = 0; j < BuyCount && j < ToBuy.Count(); j++) {
                            double setBuyPrice = (double)ToBuy[j].nprice_close * strategy.SetBuyPrice;
                            buyOrderList.Add(new BuyOrder { stock = ToBuy[j], setBuyPrice = setBuyPrice });
                            if (DEBUG) Console.WriteLine("新增買單:" + ToBuy[j].id + " 價格:" + setBuyPrice.ToString("F2") + " 加權分數:" + ToBuy[j].WeightedScore);
                        }
                    }

                    //檢查是否有需賣出目標
                    if (DEBUG) Console.WriteLine("###判斷是否要掛明日賣單###");
                    simulation.HoldingStocks.ForEach(trade => {
                        var stock = trade.stock;
                        if (DEBUG) Console.WriteLine("檢查是否需賣出:" + stock.id + " 加權分數:" + stock.WeightedScore + " 累計天數:" + stock.underSellThresholdDay);
                        if (stock.WeightedScore < strategy.SellThreshold) {
                            stock.underSellThresholdDay++;
                            if (stock.underSellThresholdDay >= strategy.SellThresholdDay) {
                                double setSellPrice = (double)stock.nprice_close * strategy.SetSellPrice;
                                sellOrderList.Add(new SellOrder { stock = stock, setSellPrice = setSellPrice });
                                if (DEBUG) Console.WriteLine("新增賣單:" + stock.id + " 價格:" + setSellPrice.ToString("F2") + " 加權分數:" + stock.WeightedScore);
                            }
                        } else {
                            stock.underSellThresholdDay = 0;
                        }
                    });
                }
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
                    Console.WriteLine($@"TradeSim: InitializeDayData {date}");
                    DataTable dataTable = sql.Select("level7",
                        new string[] { "level2.id as id",
                            "level2.Nprice_open as Nprice_open",
                            "level2.Nprice_close as Nprice_close",
                            "level2.Nprice_high as Nprice_high",
                            "level2.Nprice_low as Nprice_low" }
                        .Concat(new string[] { "min_volume_60", "max_change_abs_120" })
                        .Concat(GlobalSetting.DAYS_FP.Select(x => $"future_price_{x}"))
                        .Concat(GlobalSetting.DAYS_FR.Select(x => $"future_rank_{x}")),
                        $@"join level2 on level7.id = level2.id and level7.date = level2.date 
                        join level3 on level7.id = level3.id and level7.date = level3.date 
                        where level2.date = '{date.ToString("yyyy-MM-dd")}' order by level2.id"
                        );
                    if (dataTable.Rows.Count == 0) { continue; };
                    var dayData = new DayData() { date = date };
                    foreach (DataRow row in dataTable.Rows) {
                        string id = ((string)row["id"]).Trim();
                        dayData.StockData.Add(
                            new StockData {
                                id = id,
                                nprice_open = (decimal)row["Nprice_open"],
                                nprice_close = (decimal)row["Nprice_close"],
                                nprice_high = (decimal)row["Nprice_high"],
                                nprice_low = (decimal)row["Nprice_low"],
                                min_volume_60 = (decimal)row["min_volume_60"],
                                max_change_abs_120 = (decimal)row["max_change_abs_120"],
                                ScoreVector = GlobalSetting.DAYS_FP.Select(x => (decimal)row[$"future_price_{x}"])
                                .Concat(GlobalSetting.DAYS_FR.Select(x => (decimal)row[$"future_rank_{x}"]))
                                .ToArray()
                            }
                        );
                        if (stockInformations.ContainsKey(id)) {
                            stockInformations[id].ExpendDate(date);
                        } else {
                            stockInformations.Add(id, new StockInformation(date));
                        }
                    }
                    serialDayData.Add(dayData);
                }
            }
        }
        private static void GenerateStrategy() {
            strategyList = new List<TradeStrategy>();
            strategyList.Add(new TradeStrategy {
                WeightVevtor = new decimal[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 },
                MaxDivide = 10,
                MaxBuyInDay = 3,
                BuyThreshold = 170,
                SellThreshold = 170,
                SellThresholdDay = 2,
                SetBuyPrice = 1,
                SetSellPrice = 1
            });
        }

    }

    //contains list of stockData
    class DayData
    {
        public DateTime date;
        public List<StockData> StockData = new List<StockData>();
    }

    class StockData
    {
        public string id;
        public decimal nprice_open, nprice_close, nprice_high, nprice_low, min_volume_60, max_change_abs_120;
        public decimal[] ScoreVector;
        public double WeightedScore;
        public int underSellThresholdDay = 0;
        public override string ToString() {
            return id + " " + $@"Price{nprice_open}/{nprice_close}/{nprice_high}/{nprice_low}";
        }
    }
    class StockInformation
    {
        public DateTime StartDateInPeroid;
        public DateTime EndDateInPeroid;
        public StockInformation(DateTime setDate) {
            this.StartDateInPeroid = setDate;
            this.EndDateInPeroid = setDate;
        }
        internal void ExpendDate(DateTime date) {
            if (date > this.EndDateInPeroid) {
                this.EndDateInPeroid = date;
            }
        }
    }

    public class TradeStrategy
    {
        public decimal[] WeightVevtor;
        public int MaxDivide;
        public int MaxBuyInDay;
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
        public StockData stock;
        public DateTime BuyDate;
        public DateTime SellDate;
        public double invest;
        public double BuyPrice;
        public double SellPrice;
        public double getFund(double sellPrice) {
            return invest * sellPrice / BuyPrice;
        }
    }

    class TradeSimulation
    {
        public DateTime StartSimulationDate;
        public DateTime EndSimulationDate;
        public TradeStrategy TradeStrategy;
        public List<TradeRecord> HoldingStocks;
        public List<TradeRecord> FinishedStocks;
        public double fund;



        public TradeSimulation(TradeStrategy currentStrategy, DateTime start, DateTime end) {
            this.TradeStrategy = currentStrategy;
            this.fund = 1;
            this.HoldingStocks = new List<TradeRecord>();
            this.FinishedStocks = new List<TradeRecord>();
            this.StartSimulationDate = start;
            this.EndSimulationDate = end;
        }
    }
    class BuyOrder
    {
        public double setBuyPrice;
        public StockData stock;
    }
    class SellOrder
    {
        public double setSellPrice;
        public StockData stock;
    }
}

