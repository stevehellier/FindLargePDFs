using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FindLargePDFs
{
    class Program
    {
        static readonly int fileSize = (1024 * 1024) * 10; // 10 MB
        static int fileFoundCount = 0;
        static int fileTotalCount = 0;
        static long totalFileSize = 0;
        static string path = "";
        static IList<string> files = new List<string>();
        static ILogger logger;

        static void Main(string[] args)
        {
            if (args.Count() == 0)
            {
                Console.WriteLine($"Please specify a path to search");
                return;
            }

            if (args.Count() > 0)
            {
                path = args[0].ToString();
                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"{path} is not a valid path!");
                    return;
                }
            }

            //Console.WriteLine($"Large PDF searcher\tVersion: 0.1\tcopyright (c) 2019 Steve Hellier\n");

            //ThreadPool.SetMaxThreads(2, 0);
            logger = new Logger("test.log");

            logger.AddLogger(Loggers.Logtypes.Console);
            logger.AddLogger(Loggers.Logtypes.File);

            logger.WriteMessage($"Searching {path} for PDF files greater than {Utils.BytesToString(fileSize)}...");

            DirSeach(path);

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
                    CompressPDF(file);
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

        static void CompressPDF(object a)
        {
            string inFile = a as string;
            ConsoleSpiner spiner = new ConsoleSpiner();
            var fileName = Path.GetFileName(inFile);
            var pathName = Path.GetDirectoryName(inFile);

            var tempFolder = Path.GetTempPath();
            var outFile = Path.GetTempFileName();
            var fullTempPath = Path.Combine(tempFolder, outFile);

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = @"c:\program files\gs\gs9.27\bin\gswin64c.exe",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Normal,
                CreateNoWindow = true,
                Arguments = $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS=/ebook -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{fullTempPath}\" \"{inFile}\""
            };

            Process process = new Process
            {
                StartInfo = info
            };
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            try
            {
                var oldSize = Utils.GetFileSize(inFile);

                process.Start();
                logger.WriteMessage($"Compressing {inFile} ({Utils.BytesToString(oldSize)}) to {outFile}");
                process.WaitForExit();
                while (!process.HasExited)
                {
                    spiner.Turn();
                }
                var newSize = Utils.GetFileSize(outFile);
                float diff = Utils.CalculatePercentageDifference(oldSize, newSize);
                logger.WriteMessage($"Compressed {inFile} (was: {Utils.BytesToString(oldSize)}) (now: {Utils.BytesToString(newSize)}) (diff: {diff:n2}%)");


                //ReplaceOldFile(fullTempPath, inFile);
            }
            catch (Exception ex)
            {
                logger.WriteMessage(ex.Message);
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


        static void ReplaceOldFile(string newFile, string oldFile)
        {
            logger.WriteMessage($"Moving {newFile} to {oldFile}");
            if (File.Exists(oldFile))
            {
                var lastWriteTime = new FileInfo(oldFile).LastWriteTime;
                var creationTime = new FileInfo(oldFile).CreationTime;
                File.Move(oldFile, oldFile + ".old");
                File.SetCreationTime(newFile, creationTime);
                File.SetLastWriteTime(newFile, lastWriteTime);
            }

            File.Move(newFile, oldFile);
        }

        static void DirSeach(string directory)
        {
            ConsoleSpiner spiner = new ConsoleSpiner();
            try
            {
                foreach (var f in Directory.EnumerateFiles(directory, "*.pdf", SearchOption.AllDirectories))
                {

                    FileInfo fi = new FileInfo(f);
                    if (fi.Length > fileSize)
                    {
                        fileFoundCount++;
                        logger.WriteMessage($"Found {f}");
                        files.Add(f);
                        totalFileSize += fi.Length;
                    }
                    fileTotalCount++;
                    spiner.Turn();
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }
        }
        private static CancellationTokenSource cancelToken = new CancellationTokenSource();
    }
}

