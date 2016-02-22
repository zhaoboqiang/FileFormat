using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFormat
{
    class Program
    {
        private static string[] GetFiles(string sourceFolder, string filters, System.IO.SearchOption searchOption)
        {
            return filters.Split(',').SelectMany(filter => System.IO.Directory.GetFiles(sourceFolder, filter, searchOption)).ToArray();
        }

        private static string ProcessText(string srcText)
        {
            var text = srcText.Replace("\r\n", "\n").Replace("\r", "");
            return text;
        }

        static void ProcessDirectory(string srcDir, string dstDir, string searchPattern)
        {
            var srcFiles = GetFiles(srcDir, searchPattern, System.IO.SearchOption.AllDirectories);
            var ansiEncoding = Encoding.GetEncoding(936);

            foreach (var srcFile in srcFiles)
            {
                var dstFile = srcFile.Replace(srcDir, dstDir);
                var srcText = System.IO.File.ReadAllText(srcFile, ansiEncoding);
                var dstText = ProcessText(srcText);
                System.IO.File.WriteAllText(dstFile, dstText, ansiEncoding);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("srcpath dstpath [searchPattern]");
            }

            string searchPattern = "*.h,*.cpp,*.c";
            if (args.Length == 3) 
            {
                searchPattern = args[2];
            }

            ProcessDirectory(args[0], args[1], searchPattern);
        }
    }
}
