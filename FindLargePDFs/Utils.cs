using System;
using System.IO;

namespace FindLargePDFs
{
    public static class Utils
    {
        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public static long GetFileSize(string file)
        {
            FileInfo fi = new FileInfo(file);
            return fi.Length;
        }

        public static DateTime GetFileCreatedDate(string file)
        {
            FileInfo fi = new FileInfo(file);
            return fi.CreationTime;

        }

        public static DateTime GetFileModifiedDate(string file)
        {
            FileInfo fi = new FileInfo(file);
            return fi.LastWriteTime;

        }

        public static float CalculatePercentageDifference(long a, long b)
        {
            float DiffPercent = 0;
            float number = Math.Abs(a - b);
            float avg = ((a + b) / 2);

            float final = (number / avg);

            DiffPercent = final * 100;

            return DiffPercent;

        }
    }
}
