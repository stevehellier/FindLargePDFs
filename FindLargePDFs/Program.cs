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

            Console.WriteLine($"Searching {path} for PDF files greater than {Utils.BytesToString(fileSize)}...\n");
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
                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                    CompressPDF(file);
                });

                var stop = DateTime.Now;
                TimeSpan ts = stop.Subtract(start);
                Console.WriteLine($"Total Time elapsed: {ts.Hours:D2} Hour(s), {ts.Minutes:D2} Minute(s), {ts.Seconds:D2} Second(s)");


                //foreach (var file in files)
                //{
                //    //var t = new Thread(() => CompressPDF(file));
                //    //t.Start();
                //    //ThreadPool.QueueUserWorkItem(new WaitCallback(CompressPDF), file);

                //    CompressPDF(file);
                //    //Console.WriteLine("Continue?");
                //    //var keyPress = Console.ReadKey(true);
                //    //if (keyPress.Key == ConsoleKey.N)
                //    //{
                //    //    return;
                //    //}
                //}

                Console.WriteLine($"Found {fileFoundCount} files with total of {Utils.BytesToString(totalFileSize)} in {fileTotalCount} files");
            }
            else
            {
                Console.WriteLine("No files found!");
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

            try
            {
                process.Start();
                Console.WriteLine($"Compressing {inFile} to {outFile}");
                process.WaitForExit();
                while (!process.HasExited)
                {
                    spiner.Turn();
                }
                Console.WriteLine($"Compressed {inFile}");

                ReplaceOldFile(fullTempPath, inFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void ReplaceOldFile(string newFile, string oldFile)
        {
            Console.WriteLine($"Moving {newFile} to {oldFile}");
            if (File.Exists(oldFile))
            {
                //var owner = File.GetAccessControl(oldFile).GetOwner(typeof(System.Security.Principal.NTAccount));
                var lastWriteTime = new FileInfo(oldFile).LastWriteTime;
                var creationTime = new FileInfo(oldFile).CreationTime;
                File.Move(oldFile, oldFile + ".old");
                File.SetCreationTime(newFile, creationTime);
                File.SetLastWriteTime(newFile, lastWriteTime);
                //File.GetAccessControl(newFile).SetOwner(owner);
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
                        Console.WriteLine($"Found {f}");
                        files.Add(f);
                        totalFileSize += fi.Length;
                        fileFoundCount++;
                    }
                    fileTotalCount++;
                    spiner.Turn();
                }

                //foreach (var d in Directory.GetDirectories(directory))
                //{
                //    foreach (var f in Directory.GetFiles(d, "*.pdf"))
                //    {
                //        FileInfo fi = new FileInfo(f);
                //        if (fi.Length > fileSize)
                //        {
                //            Console.WriteLine($"Adding {f}");
                //            files.Add(f);
                //            totalFileSize += fi.Length;
                //            fileFoundCount++;
                //        }
                //        fileTotalCount++;

                //    }
                //    directoryCount++;
                //    spiner.Turn();
                //    DirSeach(d);
                //}
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }
        }
        private static CancellationTokenSource cancelToken = new CancellationTokenSource();
    }
}

