using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class ScoreManager
    {
        public static void InitializeScoreRef() {
            using (Sql sql = new Sql()) {
                sql.CreateTable("scoreRef", ScoreRef.column);
                sql.SetPrimaryKeys("scoreRef", new string[] { "fieldname", "percentileIndex" });
            }
        }
        public static void DropScoreRef() {
            using (Sql sql = new Sql()) {
                sql.DropTable("scoreRef");
            }
        }
        public static void GenerateScoreTable() {
            Console.WriteLine($@"Generating Score Table");
            List<ScoreField> fields = GenerateFeatureField();
            List<ScoreField> FPFields = GenerateFuturePriceField();
            List<ScoreField> FRFields = GenerateFutureRankField();

            using (Sql sql = new Sql()) {
                DataTable queryResult =
                    sql.Select("level6",
                    FPFields.Select(x => x.fieldName)
                    .Concat(FRFields.Select(x => x.fieldName))
                    .Concat(fields.Select(x => x.fieldName))
                    .ToArray(),
                    $@"join level3 on level6.id = level3.id and level6.date = level3.date
                        join level4 on level6.id = level4.id and level6.date = level4.date
                        join level5 on level6.id = level5.id and level6.date = level5.date
                        where min_volume_60 >= {GlobalSetting.threshold_MinVolume}
                        and max_change_abs_120 <= {GlobalSetting.threshold_MaxChange}");

                //轉存至datalist
                List<decimal[]> dataList = new List<decimal[]>();
                foreach (DataRow row in queryResult.Rows) {
                    var newRow = new decimal[queryResult.Columns.Count];
                    int i = 0;
                    foreach (DataColumn column in queryResult.Columns) {
                        newRow[i++] = (decimal)row[column];
                    }
                    dataList.Add(newRow);
                }
                List<string> colNames = new List<string>();
                foreach (DataColumn column in queryResult.Columns) {
                    colNames.Add(column.ToString());
                }
                queryResult.Dispose();
                //p = element count in a partition
                double p = (double)dataList.Count / GlobalSetting.SCORE_Partition;
                int p_int = Convert.ToInt32(Math.Round(p));
                List<ScoreRef> ScoreDataToInsert = new List<ScoreRef>();
                //依據各field排序 並計算partition內FP FR平均值
                foreach (var field in fields.Select(x => x.fieldName)) {
                    int index = colNames.IndexOf(field);
                    var orderedList = dataList.OrderBy(x => x[index]);
                    for (int i = 0; i < GlobalSetting.SCORE_Partition; i++) {
                        int startPosition = Convert.ToInt32(Math.Round(p * i));

                        ScoreRef newScoreData = new ScoreRef() {
                            fieldName = field,
                            percentileIndex = i,
                            Threshold = orderedList.ElementAt(startPosition)[index]
                        };

                        var currentPartition = orderedList
                            .Skip(startPosition)
                            .Take(p_int);
                        var FutureFields = FPFields.Select(x => x.fieldName)
                            .Concat(FRFields.Select(x => x.fieldName));
                        foreach (var futureField in FutureFields) {
                            int futureindex = colNames.IndexOf(futureField);
                            newScoreData.values[futureField] =
                                currentPartition.Select(x => x[futureindex]).Average();
                        }
                        ScoreDataToInsert.Add(newScoreData);
                    }
                }

                sql.InsertUpdateRow("scoreRef", ScoreRef.GetInsertData(ScoreDataToInsert));
            }

        }

        //決定要計算分數的欄位
        static List<ScoreField> GenerateFeatureField() {
            var result = new List<ScoreField>();
            foreach (var column in Level4.column.Where(x => x.name != "id" && x.name != "date")) {
                result.Add(new ScoreField("Level4", column.name));
            };
            return result;
        }
        //Future Price Field list
        static List<ScoreField> GenerateFuturePriceField() {
            var result = new List<ScoreField>();
            foreach (var column in Level5.column.Where(x => x.name != "id" && x.name != "date")) {
                result.Add(new ScoreField("Level5", column.name));
            };
            return result;
        }
        //Future Rank Field list
        static List<ScoreField> GenerateFutureRankField() {
            var result = new List<ScoreField>();
            foreach (var column in Level6.column.Where(x => x.name != "id" && x.name != "date")) {
                result.Add(new ScoreField("Level6", column.name));
            };
            return result;
        }
    }
    class ScoreField
    {
        public string tableName;
        public string fieldName;
        public ScoreField(string tableName, string fieldName) {
            this.tableName = tableName;
            this.fieldName = fieldName;
        }
    }
}
