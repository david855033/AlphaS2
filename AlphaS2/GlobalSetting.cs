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
        public static readonly DateTime START_DATE = new DateTime(2007, 8, 1);
        public static readonly DateTime START_CAL_DATE = new DateTime(2007, 8, 1);
        public static readonly DateTime END_DATE = new DateTime(2017, 12, 31);  // new DateTime(2017, 12, 31);

        public static int[] DAYS_BA = new int[] { 3, 5, 10, 20, 30, 40, 60, 80, 120 };
        public static int[] DAYS_MACD = new int[] { 10, 20, 40, 60 };
        public static int[] DAYS_KD = new int[] { 5, 10, 20, 40, 60 };
        public static int[] DAYS_RSI = new int[] { 10, 20, 60 };
        public static int[] DAYS_DMI = new int[] { 10, 20, 60 };
        public static int[] DAYS_FP = new int[] { 5, 10, 20, 30, 40, 50, 60, 70, 80 };
        public static int[] DAYS_FR = new int[] { 20, 40, 60, 80 };

        public static int SCORE_Partition = 20;

        public static decimal threshold_MaxChange = 0.11m;
        public static decimal threshold_MinVolume = 3000;

        public static decimal exclude_extreme_value = 0.05m;

        internal static bool MATCH_IDRULE(string x) {
            return (x.Length == 4 && !x.StartsWith("=")) || x == "=0050";
        }
    }
}
