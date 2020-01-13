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
        private static void DoDirecrtorySearchForPDFs(string directory, long MinFileSize, long MaxFileSize)
        {
            ConsoleSpiner spiner = new ConsoleSpiner();
            try
            {
                if (MinFileSize > 0)
                {
                    logger.WriteMessage($"Searching {path} for PDF files between {MinFileSize.ToFileSize()} and {MaxFileSize.ToFileSize()}...");
                }
                else
                {
                    logger.WriteMessage($"Searching {path} for PDF files up to {MaxFileSize.ToFileSize()}...");
                }

                foreach (var f in Directory.EnumerateFiles(directory, "*.pdf", SearchOption.AllDirectories))
                {
                    var filesize = Utils.GetFileSize(f);
                    if (filesize >= MinFileSize && filesize <= MaxFileSize)
                    {
                        fileFoundCount++;
                        logger.WriteMessage($"Found {f} ({filesize.ToFileSize()})");
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
