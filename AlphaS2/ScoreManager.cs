using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
            Console.WriteLine($@"Generating Score Reference Table");
            List<ScoreField> fields = GenerateFeatureField();
            List<ScoreField> FPFields = GenerateFuturePriceField();
            List<ScoreField> FRFields = GenerateFutureRankField();

            Console.WriteLine($@"Fields to calculate: {fields.Count}, loading data from SQL server....");
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
                int currentPosition = Console.CursorTop;
                Console.WriteLine($@"Transform Data...");
                List<decimal[]> dataList = new List<decimal[]>();
                foreach (DataRow row in queryResult.Rows) {
                    var newRow = new decimal[queryResult.Columns.Count];
                    int i = 0;
                    foreach (DataColumn column in queryResult.Columns) {
                        newRow[i++] = (decimal)row[column];
                    }
                    dataList.Add(newRow);
                    if (i % 100 == 0) {
                        Console.CursorTop = currentPosition;
                        Console.WriteLine($@"Transform Data...{i}00/{queryResult.Rows.Count}         ");
                    }
                }
                Console.CursorTop = currentPosition;
                Console.WriteLine($@"Transform Data...{queryResult.Rows.Count}/{queryResult.Rows.Count}         ");

                List<string> colNames = new List<string>();
                foreach (DataColumn column in queryResult.Columns) {
                    colNames.Add(column.ToString());
                }
                queryResult.Dispose();
                //p = element count in a partition
                double p = (double)dataList.Count / GlobalSetting.SCORE_Partition;
                int p_int = Convert.ToInt32(Math.Round(p));
                List<ScoreRef> ScoreDataToInsert = new List<ScoreRef>();

                currentPosition = Console.CursorTop;
                int count = 0;
                //依據各field排序 並計算partition內FP FR平均值
                foreach (var field in fields.Select(x => x.fieldName)) {
                    Console.CursorTop = currentPosition;
                    Console.WriteLine($@"Caculating Field: {field} ({++count}/{fields.Count})                   ");
                    int index = colNames.IndexOf(field);
                    var orderedList = dataList.OrderBy(x => x[index]);
                    for (int i = 0; i < GlobalSetting.SCORE_Partition; i++) {
                        Console.CursorTop = currentPosition + 1;
                        Console.WriteLine($@"   Partition: {i + 1}/{GlobalSetting.SCORE_Partition}");
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
                            int ExtrmeValueN = Convert.ToInt32(currentPartition.Count() * GlobalSetting.exclude_extreme_value);
                            var excludeExtremeValue =
                                currentPartition
                                .Skip(ExtrmeValueN)
                                .Take(currentPartition.Count() - ExtrmeValueN * 2);
                            newScoreData.values[futureField] =
                                excludeExtremeValue.Select(x => x[futureindex]).Average();
                        }
                        ScoreDataToInsert.Add(newScoreData);
                    }
                }
                Console.WriteLine("Score Reference Calculation Done");
                sql.InsertUpdateRow("scoreRef", ScoreRef.GetInsertData(ScoreDataToInsert));
            }

        }

        public static void ExportScoreRef() {
            using (Sql sql = new Sql()) {
                List<ScoreRef> scoreRefData = ScoreRef.DataAdaptor(sql.Select("scoreref"));
                var path = GlobalSetting.SCORE_REF_PATH + $@"\scoreRef.txt";
                var toWrite = new StringBuilder();
                toWrite.AppendLine(String.Join(",", ScoreRef.column.Select(x => x.name)));
                foreach (var row in scoreRefData) {
                    toWrite.AppendLine(String.Join(",", new string[] {
                        row.fieldName,
                        row.percentileIndex.ToString(),
                        String.Join(",",row.values.Values)
                    }));
                }
                using (var sw = new StreamWriter(path)) {
                    Console.WriteLine($@"export scoreRef: {path}");
                    sw.Write(toWrite);
                }
            }
        }
        public static void ImportScoreRef() {
            var path = GlobalSetting.SCORE_REF_PATH + $@"\scoreRef.txt";
            if (!File.Exists(path)) { Console.WriteLine($@"not found: {path}"); return; }
            using (var sr = new StreamReader(path))
            using (Sql sql = new Sql()) {
                Console.WriteLine($@"loading {path}");
                string data = sr.ReadToEnd();
                string[] splitted = data.Split('\n');
                List<ScoreRef> ScoreDataToInsert = new List<ScoreRef>();
                string[] cols = splitted[0].Split(',');
                for (int i = 1; i < splitted.Length; i++) {
                    string[] splittedRow = splitted[i].Split(',');
                    if (splittedRow.Length==1) { continue; }
                    var newScoreRef = new ScoreRef() {
                        fieldName = splittedRow[0],
                        percentileIndex = Convert.ToInt32(splittedRow[1]),
                        Threshold = Convert.ToDecimal(splittedRow[2])
                    };
                    for (var j = 3; j < splittedRow.Length; j++) {
                        newScoreRef.values.Add(cols[j], Convert.ToDecimal(splittedRow[j]));
                    }
                    ScoreDataToInsert.Add(newScoreRef);
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
