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
        static readonly string FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\alphas2";
        public static void CheckDirectory() {
            if (!Directory.Exists(FOLDER_PATH)) {
                Directory.CreateDirectory(FOLDER_PATH);
                Console.WriteLine($@"create dir: {FOLDER_PATH}");
            } else {
                Console.WriteLine($@"{FOLDER_PATH} Exists.");
            }
        }

        public static void WriteToFile(string fileName, string content, bool append = false) {
            using (var sw = new StreamWriter(FOLDER_PATH+$@"\{fileName}.txt", append, Encoding.Default)) {
                sw.Write(content);
                Console.WriteLine($"write to {fileName}.txt,  append = {append}");
            }
        }
    }
}
