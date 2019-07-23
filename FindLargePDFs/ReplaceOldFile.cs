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
        private static void DoReplaceOldFile(string newFile, string oldFile)
        {
            logger.WriteMessage($"Moving {newFile} to {oldFile}");
            if (File.Exists(oldFile))
            {
                var lastWriteTime = new FileInfo(oldFile).LastWriteTime;
                var creationTime = new FileInfo(oldFile).CreationTime;
                try
                {
                    File.Move(oldFile, oldFile + ".old");
                    File.SetCreationTime(newFile, creationTime);
                    File.SetLastWriteTime(newFile, lastWriteTime);
                }
                catch (Exception ex)
                {
                    logger.WriteMessage(ex.Message);
                }
            }
            else
            {
                try
                {
                    File.Move(newFile, oldFile);
                }
                catch (Exception ex)
                {
                    logger.WriteMessage(ex.Message);
                }
            }
        }
    }
}
