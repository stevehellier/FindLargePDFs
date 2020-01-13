using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace FindLargePDFs
{
    partial class Program
    {
        public class Options
        {
            [Option('v', Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('p', Required = true, HelpText = "Path to search")]
            public string Path { get; set; }

            [Option("min", Required = false, HelpText = "Minimum file size")]
            public long Min { get; set; }
            [Option("max", Required = false, HelpText = "Maximum file size")]
            public long Max { get; set; }
        }

        //private static long MaxFileSize = (1024 * 1024) * 30; // 10 MB
        private static long MaxFileSize = long.MaxValue;
        private static long MinFileSize = 0;
        private static readonly string logfileName = "log.txt";


        private static int fileFoundCount = 0;
        private static int fileTotalCount = 0;
        private static long totalFileSize = 0;
        private static string path = "";

        private static IList<string> files = new List<string>();
        private static ILogger logger;
        private static DateTime searchDate = DateTime.Now.AddYears(-5);

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    if (o.Verbose)
                    {
                        Console.WriteLine($"Verbose mode!!");
                    }
                    if (!string.IsNullOrEmpty(o.Path))
                    {
                        path = o.Path;
                    }
                    else
                    {
                        Console.WriteLine(o.ToString());
                    }

                    if (o.Min > 0)
                    {
                        MinFileSize = o.Min;
                    }
                    if (o.Max > 0)
                    {
                        MaxFileSize = o.Max;
                    }

                });
            //Console.WriteLine($"Min file is {MinFileSize.ToFileSize()}");
            //var MaxFileSizeDisplayName = MaxFileSize.ToFileSize();
            //Console.WriteLine($"Max file is {MaxFileSizeDisplayName}");


            //return;

            if (args.Count() == 0)
            {
                Console.WriteLine($"Please specify a path to search");
                return;
            }

            //if (args.Count() > 0)
            //{
            //    path = args[0].ToString();
            //    if (!Directory.Exists(path))
            //    {
            //        Console.WriteLine($"{path} is not a valid path!");
            //        return;
            //    }
            //}

            //Console.WriteLine($"Large PDF searcher\tVersion: 0.1\tcopyright (c) 2019 Steve Hellier\n");

            logger = new Logger(logfileName);

            logger.AddLogger(Loggers.Logtypes.Console);
            logger.AddLogger(Loggers.Logtypes.File);

            DoDirecrtorySearchForPDFs(path, MinFileSize, MaxFileSize);

            //DoDirecrtorySearchByDate(path, searchDate);

            ParallelOptions parallelOptions = new ParallelOptions
            {
                CancellationToken = cancelToken.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            if (files.Count() > 0)
            {
                var start = DateTime.Now;
                Parallel.ForEach(files, file =>
                {
                    new ParallelOptions { MaxDegreeOfParallelism = 2, CancellationToken = cancelToken.Token };
                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                    //DoCompressPDF(file);
                });

                var stop = DateTime.Now;
                TimeSpan ts = stop.Subtract(start);
                logger.WriteMessage($"Total Time elapsed: {ts.Hours:D2} Hour(s), {ts.Minutes:D2} Minute(s), {ts.Seconds:D2} Second(s)");

                logger.WriteMessage($"Found {fileFoundCount} files with total of {Utils.BytesToString(totalFileSize)} in {fileTotalCount} files");
            }
            else
            {
                logger.WriteMessage("No files found!");
            }
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.WriteMessage(e.Data);
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.WriteMessage(e.Data);
        }

        private static CancellationTokenSource cancelToken = new CancellationTokenSource();
    }
}

