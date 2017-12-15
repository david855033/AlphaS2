﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    class Level2
    {
        public string id;
        public DateTime date;
        public decimal amount_per_trade;
        public decimal divide;
        public decimal fix;
        public decimal price_mean;
        public decimal Nprice_mean;
        public decimal Nprice_open;
        public decimal Nprice_close;
        public decimal Nprice_high;
        public decimal Nprice_low;

        public static List<SqlColumn> column =
         new List<SqlColumn> {
                new SqlColumn("id","nchar(10)",false),
                new SqlColumn("date","date",false),
                new SqlColumn("amount_per_trade","decimal(19,0)",false),
                new SqlColumn("divide","decimal(9,4)",false),
                new SqlColumn("fix","decimal(9,4)",false),
                new SqlColumn("price_mean","decimal(9,2)",false),
                new SqlColumn("Nprice_mean","decimal(9,2)",false),
                new SqlColumn("Nprice_open","decimal(9,2)",false),
                new SqlColumn("Nprice_close","decimal(9,2)",false),
                new SqlColumn("Nprice_high","decimal(9,2)",false),
                new SqlColumn("Nprice_low","decimal(9,2)",false)
         };
        public static List<Level2> DataAdaptor(DataTable dataTableLevel2) {
            var result = new List<Level2>();
            foreach (DataRow row in dataTableLevel2.Rows) {
                result.Add(new Level2() {
                    id = ((string)row["id"]).Trim(),
                    date = (DateTime)row["date"],
                    amount_per_trade = (decimal)row["amount_per_trade"],
                    divide = (decimal)row["divide"],
                    fix = (decimal)row["fix"],
                    price_mean = (decimal)row["price_mean"],
                    Nprice_mean = (decimal)row["Nprice_mean"],
                    Nprice_open = (decimal)row["Nprice_open"],
                    Nprice_close = (decimal)row["Nprice_close"],
                    Nprice_high = (decimal)row["Nprice_high"],
                    Nprice_low = (decimal)row["Nprice_low"]
                });
            }
            return result;
        }

        public static SqlInsertData GetInsertData(List<Level2> level2DataToInsert) {
            SqlInsertData result = new SqlInsertData();
            result.ColumnList = Level2.column;
            result.primaryKeys = new List<string>() { "id", "date" };
            foreach (var data in level2DataToInsert) {
                result.DataList.Add(new object[] {
                data.id, data.date,data.amount_per_trade, data.divide, data.fix, data.price_mean,
                 data.Nprice_mean, data.Nprice_open,data.Nprice_close, data.Nprice_high,data.Nprice_low });
            }
            return result;
        }
    }
}
