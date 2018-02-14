using System;
using System.Collections.Generic;
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
                sql.SetPrimaryKey("scoreRef", "fieldname");
            }
        }
        public static void DropScoreRef() {
            using (Sql sql = new Sql()) {
                sql.DropTable("scoreRef");
            }
        }
        public static void GenerateScoreTable() {
            Console.WriteLine($@"Generating Score Table");
            List<ScoreField> fields = GenerateScoreFieldList();
            foreach (var field in fields) {

            }
            //驗證資料(changeabs min_volume 在level3內)
        }

        //決定要計算分數的欄位
        static List<ScoreField> GenerateScoreFieldList() {
            var ScoreFieldList = new List<ScoreField>();
            foreach (var column in Level4.column.Where(x => x.name != "id" && x.name != "date")) {
                ScoreFieldList.Add(new ScoreField("Level4", column.name));
            };

            return ScoreFieldList;
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
