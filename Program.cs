using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace FileFormat
{
    class Options
    {
        [Option('s', "src", Required = true, HelpText = "source directory.")]
        public string srcDir { get; set; }

        [Option('d', "dst", Required = true, HelpText = "destination directory.")]
        public string dstDir { get; set; }

        [Option('e', "ext", DefaultValue = "*.h,*.cpp,*.c", HelpText = "extension search pattern.")]
        public string extension { get; set; }

        [Option('u', "utf", DefaultValue = false, HelpText = "convert to utf8.")]
        public bool convertToUtf8 { get; set; }

        [Option('c', "src_cp", DefaultValue = 936, HelpText = "source code page.")]
        public int srcCodepage { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        private static string[] GetFiles(string sourceFolder, string filters, System.IO.SearchOption searchOption)
        {
            return filters.Split(',').SelectMany(filter => System.IO.Directory.GetFiles(sourceFolder, filter, searchOption)).ToArray();
        }

        private static Encoding GetTextEncoding(string filename, Ude.CharsetDetector charsetDetector)
        {
            var encoding = Encoding.UTF8;

            using (FileStream fs = File.OpenRead(filename))
            {
                charsetDetector.Feed(fs);
                charsetDetector.DataEnd();

                try
                {
                    encoding = Encoding.GetEncoding(charsetDetector.Charset);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to obtain encoding of the file to {0}. Corrupted characters may occur. {1}", charsetDetector.Charset, ex.Message);
                }
            }

            return encoding;
        }

        private static string ProcessText(string srcText)
        {
            // LF (Line feed, 0x0A, \r)
            // CR (Carriage return, 0x0D, \n) 

            var rCount = srcText.Count(c => c == '\r');
            var nCount = srcText.Count(c => c == '\n');

            if (rCount > 0 && nCount > 0)
            {
                return srcText.Replace("\r\n", "\n").Replace("\r", "");
            }

            if (rCount > 0)
            {
                return srcText.Replace("\r", "\n");
            }

            return srcText;
        }

        static void ProcessDirectory(string srcDir, string dstDir, string searchPattern, int srcCodepage, bool convertToUtf8)
        {
            var srcFiles = GetFiles(srcDir, searchPattern, System.IO.SearchOption.AllDirectories);
            var utf8Encoding = Encoding.UTF8;
            Ude.CharsetDetector charsetDetector = new Ude.CharsetDetector();

            bool sameDir = srcDir == dstDir;

            foreach (var srcFile in srcFiles)
            {
                var srcEncoding = GetTextEncoding(srcFile, charsetDetector);
                var dstFile = srcFile.Replace(srcDir, dstDir);
                var srcText = File.ReadAllText(srcFile, srcEncoding);
                var dstText = ProcessText(srcText);

                if (!sameDir || srcText != dstText)
                {
                    if (convertToUtf8)
                    {
                        System.IO.File.WriteAllText(dstFile, dstText, utf8Encoding);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(dstFile, dstText, srcEncoding);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                ProcessDirectory(options.srcDir, options.dstDir, options.extension, options.srcCodepage, options.convertToUtf8);
            } 
        }
    }
}
