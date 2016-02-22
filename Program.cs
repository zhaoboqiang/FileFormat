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

        private static void ProcessFile(UTF8Encoding utf8Encoding, string path)
        {
            var text = System.IO.File.ReadAllText(path);
            text.Replace("\r\n", "\n");

            System.IO.File.WriteAllText(path, text, utf8Encoding);
        }

        static void ProcessDirectory(string path, string searchPattern)
        {
            var srcs = GetFiles(path, searchPattern, System.IO.SearchOption.AllDirectories);
            var utf8Encoding = new System.Text.UTF8Encoding(true);

            foreach (var src in srcs)
            {
                ProcessFile(utf8Encoding, src);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("path [searchPattern]");
            }

            string searchPattern = "*.h,*.cpp,*.c";
            if (args.Length == 2) 
            {
                searchPattern = args[1];
            }

            ProcessDirectory(args[0], searchPattern);
        }
    }
}
