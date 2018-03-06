using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace AlphaS2
{
    static class FileWriter
    {
        public static void CheckDirectory() {
            foreach (var path in new[] { GlobalSetting.FOLDER_PATH, GlobalSetting.TRADE_SIM_PATH }) {
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                    Console.WriteLine($@"create dir: {path}");
                } else {
                    Console.WriteLine($@"{path} Exists.");
                }
            }
        }

        public static void WriteToFile(string fileName, string content, bool append = false) {
            using (var sw = new StreamWriter(GlobalSetting.FOLDER_PATH + $@"\{fileName}.txt", append, Encoding.Default)) {
                sw.Write(content);
                Console.WriteLine($"write to {fileName}.txt,  append = {append}");
            }
        }
    }
}
