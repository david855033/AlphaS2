using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace AlphaS2
{
    static class Advicer
    {
        public static void MakeAdvice() {
            Console.WriteLine($@"Generating advice");
            using (Sql sql = new Sql()) {
                DataTable queryLastDate = sql.Select("fetch_log",
                    new[] { "date" },
                    new[] { "type = 'A'", "uploaded=1" },
                    "order by date desc");
                if (queryLastDate.Rows.Count == 0) {
                    Console.WriteLine("no available data");
                    return;
                }
                var date = (DateTime)queryLastDate.Rows[0].ItemArray[0];

                string queryStr =
                    $@"(select id, name, date, price_open, price_close, price_high, price_low, divide from level1 where level1.date = '{date.ToString("yyyy-MM-dd")}') as l1query
inner join(select id, volume from level2 where level2.date = '{date.ToString("yyyy-MM-dd")}') as l2query on l1query.id = l2query.id
inner join(select id, min_volume_60, max_change_abs_120 from level3 where level3.date = '{date.ToString("yyyy-MM-dd")}') as l3query on l1query.id = l3query.id
inner join(select id, future_price_5+future_price_10 + future_price_15 + future_price_20 + future_price_30 + future_price_40 + future_price_60 + future_price_80 as weightedScore from level7 where level7.date = '{date.ToString("yyyy-MM-dd")}') as l7query on l1query.id = l7query.id
order by weightedScore desc";

                DataTable query = sql.Select(queryStr,
                    new[] { "l1query.id", "name", "weightedScore"
                    ,"volume", "price_open", "price_close", "price_high", "price_low", "divide", "min_volume_60", "max_change_abs_120" });

                var advices = new List<AdviceRow>();
                foreach (DataRow row in query.Rows) {
                    DataTable maxWeight8Query = sql.Select(
                        $@"(select top 8 date,future_price_5+future_price_10+future_price_15+future_price_20+future_price_30+future_price_40+future_price_60+future_price_80 as weightedScore
from level7 where id='{((string)row["id"]).Trim()}' order by date desc) as weightedScoreMax", new[] { "max(weightedScore)" });
                    decimal maxWeightScore = 0;
                    if (maxWeight8Query.Rows.Count > 0) {
                        maxWeightScore = (decimal)maxWeight8Query.Rows[0][0];
                    }

                    advices.Add(
                    new AdviceRow {
                        id = ((string)row["id"]).Trim(),
                        name = ((string)row["name"]).Trim(),
                        weightedScore = (decimal)row["weightedScore"],
                        maxWeightScore = maxWeightScore,
                        volume = (decimal)row["volume"],
                        price_open = (decimal)row["price_open"],
                        price_close = (decimal)row["price_close"],
                        price_high = (decimal)row["price_high"],
                        price_low = (decimal)row["price_low"],
                        min_volume_60 = (decimal)row["min_volume_60"],
                        max_change_abs_120 = (decimal)row["max_change_abs_120"]
                    }
                    );
                }
                advices.Sort((a, b) => {
                    decimal wa = a.weightedScore, wb = b.weightedScore;
                    if (a.min_volume_60 > GlobalSetting.threshold_MinVolume && a.max_change_abs_120 < GlobalSetting.threshold_MaxChange) {
                        wa += 10000;
                    }
                    if (b.min_volume_60 > GlobalSetting.threshold_MinVolume && b.max_change_abs_120 < GlobalSetting.threshold_MaxChange) {
                        wb += 10000;
                    }
                    return -wa.CompareTo(wb);
                });
                StringBuilder report = new StringBuilder();
                report.AppendLine(String.Join("\t", new string[] {
                "stockID","stockName","weightedScore","maxWeightScore8","suggestAction"
                ,"suggestBuyPrice","volume(k)","open"
                ,"high","low","close","divide"
                ,"min_volume_60","max_change_abs_120",$"(資料日期:{date.ToShortDateString()})"}));

                foreach (var adviceRow in advices) {
                    report.AppendLine(string.Join("\t", new string[] {
                        adviceRow.id,
                        adviceRow.name,
                        adviceRow.weightedScore.ToString(),
                        adviceRow.maxWeightScore.ToString(),
                        adviceRow.weightedScore>=60?"BUY"
                        :adviceRow.maxWeightScore < 15?"SELL":"",
                        ((double)adviceRow.price_close*1.025).ToString("F2"),
                        adviceRow.volume.ToString(),
                        adviceRow.price_open.ToString(),
                        adviceRow.price_high.ToString(),
                        adviceRow.price_low.ToString(),
                        adviceRow.price_close.ToString(),
                        adviceRow.divide.ToString(),
                        adviceRow.min_volume_60.ToString(),
                        adviceRow.max_change_abs_120.ToString()
                    }));
                }

                using (var sw = new StreamWriter(GlobalSetting.REPORT_PATH + $@"\{date.ToString("yyyyMMdd")}.txt")) {
                    sw.Write(report.ToString());
                }


            }
        }
    }
    class AdviceRow
    {
        public string id;
        public string name;
        public decimal weightedScore;
        public decimal maxWeightScore;
        public decimal volume;
        public decimal price_open;
        public decimal price_close;
        public decimal price_high;
        public decimal price_low;
        public decimal divide;
        public decimal min_volume_60;
        public decimal max_change_abs_120;
    }
}
