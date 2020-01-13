using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindLargePDFs
{
    partial class Program
    {
        private static void DoDirecrtorySearchForPDFs(string directory)
        {
            ConsoleSpiner spiner = new ConsoleSpiner();
            try
            {
                logger.WriteMessage($"Searching {path} for PDF files greater than {Utils.BytesToString(MaxFileSize)}...");
                foreach (var f in Directory.EnumerateFiles(directory, "*.pdf", SearchOption.AllDirectories))
                {
                    var filesize = Utils.GetFileSize(f);
                    if (filesize >= MaxFileSize)
                    //FileInfo fi = new FileInfo(f);
                    //if (fi.Length > fileSize)
                    {
                        fileFoundCount++;
                        logger.WriteMessage($"Found {f} ({filesize / (1024 * 1024)} MB)");
                        files.Add(f);
                        totalFileSize += filesize;
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
        private static void DoDirecrtorySearchByDate(string directory, DateTime date)
        {
            ConsoleSpiner spiner = new ConsoleSpiner();
            try
            {
                logger.WriteMessage($"Searching for files older than {date.ToLongDateString()}");
                //Console.WriteLine();
                //System.Threading.Thread.Sleep(5000)ead.Sleep(5000)

                foreach (var f in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
                {
                    if (!f.ToLower().Contains("thumbs.db"))
                    {
                        var createdDate = Utils.GetFileCreatedDate(f);
                        if (createdDate <= date)
                        //FileInfo fi = new FileInfo(f);
                        //if (fi.Length > fileSize)
                        {
                            fileFoundCount++;
                            var filesize = Utils.GetFileSize(f);
                            logger.WriteMessage($"Found {f}");
                            files.Add(f);
                            totalFileSize += filesize;
                        }
                        fileTotalCount++;
                        spiner.Turn();
                    }
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }
        }
    }
}
