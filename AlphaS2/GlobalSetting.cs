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
        public static readonly DateTime END_DATE = new DateTime(2007, 12, 31);
    }
}
