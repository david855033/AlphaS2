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
            if (!Directory.Exists(GlobalSetting.FOLDER_PATH)) {
                Directory.CreateDirectory(GlobalSetting.FOLDER_PATH);
                Console.WriteLine($@"create dir: {GlobalSetting.FOLDER_PATH}");
            } else {
                Console.WriteLine($@"{GlobalSetting.FOLDER_PATH} Exists.");
            }
        }

        public static void WriteToFile(string fileName, string content, bool append = false) {
            using (var sw = new StreamWriter(GlobalSetting.FOLDER_PATH +$@"\{fileName}.txt", append, Encoding.Default)) {
                sw.Write(content);
                Console.WriteLine($"write to {fileName}.txt,  append = {append}");
            }
        }
    }
}
