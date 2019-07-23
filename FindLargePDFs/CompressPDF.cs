using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindLargePDFs
{
    partial class Program
    {
        private static void CompressPDF(object a)
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
                logger.WriteMessage($"Compressing {inFile} to {outFile}");
                process.WaitForExit();
                while (!process.HasExited)
                {
                    spiner.Turn();
                }
                var newSize = Utils.GetFileSize(outFile);
                float diff = Utils.CalculatePercentageDifference(oldSize, newSize);
                logger.WriteMessage($"Compressed {inFile} (was: {Utils.BytesToString(oldSize)}) (now: {Utils.BytesToString(newSize)}) (diff: {diff:n2}%)");


                ReplaceOldFile(fullTempPath, inFile);
            }
            catch (Exception ex)
            {
                logger.WriteMessage(ex.Message);
            }
        }
    }
}
