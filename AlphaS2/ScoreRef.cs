﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class ScoreRef
    {
        public string fieldName;
        public int percentileIndex;
        private decimal _threshold;
        public decimal Threshold { get => Math.Round(_threshold, 4); set => _threshold = value; }

        public Dictionary<string, decimal> values = new Dictionary<string, decimal>();

        public static List<SqlColumn> column;

        public static void Initiate() {
            var newColumns = new List<SqlColumn>() {
                    new SqlColumn("fieldname","nchar(30)",false),
                    new SqlColumn("percentileIndex","tinyint",false),
                    new SqlColumn("threshold","decimal(9,2)",false)
                };

            foreach (var d in GlobalSetting.DAYS_FP) {
                newColumns.Add(new SqlColumn($@"future_price_{d}", "decimal(9,2)", false));
            }
            ScoreRef.column = newColumns;
        }

        public static List<ScoreRef> DataAdaptor(DataTable scoreRefTable) {
            var result = new List<ScoreRef>();
            foreach (DataRow row in scoreRefTable.Rows) {
                var newScoreRefField = new ScoreRef() {
                    fieldName = ((string)row["fieldname"]).Trim(),
                    percentileIndex = Convert.ToInt32(row["percentileIndex"]),
                    Threshold = (decimal)row["threshold"]
                };
                foreach (string c in column.Select(x => x.name)
                    .Where(x => x != "fieldname" && x != "percentileIndex" && x != "threshold")) {
                    newScoreRefField.values[c] = (decimal)row[c];
                }
                result.Add(newScoreRefField);
            }
            return result;
        }

        public static SqlInsertData GetInsertData(List<ScoreRef> scoreRefTableToInsert) {
            SqlInsertData result = new SqlInsertData {
                ColumnList = ScoreRef.column,
                primaryKeys = new List<string>() { "fieldname", "percentileIndex" }
            };
            foreach (var data in scoreRefTableToInsert) {
                var newObjects = new List<object>() {
                    data.fieldName, data.percentileIndex,data.Threshold
                };
                foreach (string c in column.Select(x => x.name)) {
                    if (c == "fieldname" || c == "percentileIndex" || c == "threshold") { continue; }
                    if (data.values.TryGetValue(c, out decimal v)) {
                        newObjects.Add(Math.Round(v, 4));
                    } else {
                        newObjects.Add(-1000);
                    }
                }
                result.DataList.Add(newObjects.ToArray());
            }
            return result;
        }
    }
}
