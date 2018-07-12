using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class Advicer
    {
        public static void MakeAdvice() {
            Console.WriteLine($@"Generating advice");
            using (Sql sql = new Sql()) {
                DataTable queryLastDate= sql.Select("fetch_log",
                    new[] { "date" },
                    new[] { "type = 'A'", "uploaded=1" },
                    "order by date desc");

                if (queryLastDate.Rows.Count > 0) {
                     var date= (DateTime)queryLastDate.Rows[0].ItemArray[0];
                }

                
            }
        }
    }
}
