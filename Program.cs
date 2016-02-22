using System;
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

        private static string ProcessText(string srcText)
        {
            var text = srcText.Replace("\r\n", "\n").Replace("\r", "");
            return text;
        }

        static void ProcessDirectory(string srcDir, string dstDir, string searchPattern, int srcCodepage)
        {
            var srcFiles = GetFiles(srcDir, searchPattern, System.IO.SearchOption.AllDirectories);
            var ansiEncoding = Encoding.GetEncoding(srcCodepage);

            bool sameDir = srcDir == dstDir;

            foreach (var srcFile in srcFiles)
            {
                var dstFile = srcFile.Replace(srcDir, dstDir);
                var srcText = System.IO.File.ReadAllText(srcFile, ansiEncoding);
                var dstText = ProcessText(srcText);

                if (sameDir && srcText != dstText)
                {
                    System.IO.File.WriteAllText(dstFile, dstText, ansiEncoding);
                }
            }
        }

        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                ProcessDirectory(options.srcDir, options.dstDir, options.extension, options.srcCodepage);
            } 
        }
    }
}
