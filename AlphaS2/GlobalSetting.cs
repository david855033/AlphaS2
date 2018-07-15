using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaS2
{
    static class GlobalSetting
    {
        public static readonly string FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\alphas2data";
        public static readonly string REPORT_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\alphas2data\report";
        public static readonly string TRADE_SIM_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\alphas2data\tradeSim";

        public static readonly DateTime START_DATE = new DateTime(2007, 8, 1);     //download start date(2007,8,1)
        public static readonly DateTime START_CAL_DATE = new DateTime(2007, 8, 1);   //level1 start date
        public static readonly DateTime END_DATE = new DateTime(2007, 7, 15);  // DateTime.Now;

        public static readonly DateTime START_SIM_DATE = new DateTime(2016, 1, 1); // TESTING
        public static readonly DateTime END_SIM_DATE = new DateTime(2017, 12, 31);  // TESTING

        public static readonly string SCORE_REF_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\alphas2data\scoreRef";

        public static int[] DAYS_BA = new int[] { 3, 5, 10, 20, 30, 40, 60, 80, 120 };
        public static int[] DAYS_MACD = new int[] { 10, 20, 40, 60 };
        public static int[] DAYS_KD = new int[] { 5, 10, 20, 40, 60 };
        public static int[] DAYS_RSI = new int[] { 10, 20, 60 };
        public static int[] DAYS_DMI = new int[] { 10, 20, 60 };
        public static int[] DAYS_FP = new int[] { 5, 10, 15, 20, 30, 40, 60, 80 };
        public static int[] DAYS_FR = new int[] { };

        public static int SCORE_Partition = 20;

        public static decimal threshold_MaxChange = 11m;  //單位為%
        public static decimal threshold_MinVolume = 3000;

        public static decimal exclude_extreme_value = 0.05m;

        internal static bool MATCH_IDRULE(string x) {
            return (x.Length == 4 && !x.StartsWith("=")) || x == "=0050";
        }
    }
}

