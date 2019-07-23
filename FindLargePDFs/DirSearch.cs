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
        private static void DoDirecrtorySearch(string directory)
        {
            ConsoleSpiner spiner = new ConsoleSpiner();
            try
            {
                foreach (var f in Directory.EnumerateFiles(directory, "*.pdf", SearchOption.AllDirectories))
                {
                    var filesize = Utils.GetFileSize(f);
                    if (filesize > MaxFileSize)
                    //FileInfo fi = new FileInfo(f);
                    //if (fi.Length > fileSize)
                    {
                        fileFoundCount++;
                        logger.WriteMessage($"Found {f}");
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
    }
}
