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
                sql.SetPrimaryKey("scoreRef",  "fieldname");
            }
        }
        public static void DropScoreRef() {
            using (Sql sql = new Sql()) {
                sql.DropTable("scoreRef");
            }
        }
        public static void GenerateScoreTable() {

        }
    }
}
