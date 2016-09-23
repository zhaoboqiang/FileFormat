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
        [Option('i', "input", Required = true, HelpText = "input directory.")]
        public string srcDir { get; set; }

        [Option('o', "output", DefaultValue = "", HelpText = "output directory.")]
        public string dstDir { get; set; }

        [Option('e', "ext", DefaultValue = "*.h,*.cpp,*.c", HelpText = "extension search pattern.")]
        public string extension { get; set; }

        [Option('c', "cp", DefaultValue = 936, HelpText = "default code page.")]
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

        private static Encoding GetTextEncoding(string filename, Ude.CharsetDetector charsetDetector, Encoding srcEncoding)
        {
            var encoding = Encoding.UTF8;

            using (FileStream fs = File.OpenRead(filename))
            {
                charsetDetector.Reset();
                charsetDetector.Feed(fs);
                charsetDetector.DataEnd();

                try
                {
                    encoding = Encoding.GetEncoding(charsetDetector.Charset);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to obtain encoding of the file to {0}. Using source encoding instead. Corrupted characters may occur. {1}", charsetDetector.Charset, ex.Message);
                    
                    encoding = srcEncoding;
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

        static bool IsByteArrayEquals(byte[] a1, int offset1, byte[] a2, int offset2)
        {
            int n1 = a1.Length - offset1;
            int n2 = a2.Length - offset2;
            
            if (n1 != n2)
            {
                return false;
            }

            int i1 = offset1;
            int i2 = offset2;
            
            while (n1-- > 0)
            {
                if (a1[i1++] != a2[i2++])
                {
                    return false;
                }
            }

            return true;
        }

        static void SaveText(string srcFile, string dstFile, string text)
        {
            var utf8Encoding = new UTF8Encoding();

            var srcBytes = File.ReadAllBytes(srcFile);
            var dstBytes = utf8Encoding.GetBytes(text);

            int offset = 0;
            
            if (srcBytes.Length == dstBytes.Length + 3 && (srcBytes[0] == 0xEF && srcBytes[1] == 0xBB && srcBytes[2] == 0xBF))
            {
                offset = 3;
            }

            if (!IsByteArrayEquals(srcBytes, offset, dstBytes, 0))
            {
                var dstPath = Path.GetDirectoryName(dstFile);
                Directory.CreateDirectory(dstPath);
                System.IO.File.WriteAllText(dstFile, text, Encoding.UTF8);
            }
        }

        static void ProcessDirectory(string srcDir, string dstDir, string searchPattern, int srcCodepage)
        {
            if (string.IsNullOrEmpty(dstDir))
            {
                dstDir = srcDir;
            }

            var srcFiles = GetFiles(srcDir, searchPattern, System.IO.SearchOption.AllDirectories);
            var srcEncoding = Encoding.GetEncoding(srcCodepage);
            Ude.CharsetDetector charsetDetector = new Ude.CharsetDetector();

            foreach (var srcFile in srcFiles)
            {
                var encoding = GetTextEncoding(srcFile, charsetDetector, srcEncoding);
                var dstFile = srcFile.Replace(srcDir, dstDir);
                var srcText = File.ReadAllText(srcFile, encoding);
                var dstText = ProcessText(srcText);

                SaveText(srcFile, dstFile, dstText);
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
