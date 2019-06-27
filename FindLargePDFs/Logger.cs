using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindLargePDFs
{
    class Logger : ILogger
    {
        private string _fileName { get; set; }
        private List<Loggers.Logtypes> _loggers { get; set; }

        private static readonly object _syncObject = new object();

        public Logger(string Filename)
        {
            this._loggers = new List<Loggers.Logtypes>();
            this._fileName = Filename;
        }

        public void AddLogger(Loggers.Logtypes logger)
        {
            _loggers.Add(logger);
        }

        public void WriteMessage(string Message)
        {
            foreach (var logger in _loggers)
            {
                switch (logger)
                {
                    case Loggers.Logtypes.Console:
                        WriteConsole(Message);
                        break;
                    case Loggers.Logtypes.File:
                        WriteLogFile(Message);
                        break;
                    default:
                        WriteConsole(Message);
                        break;
                }
            }
        }

        private void WriteConsole(string Message)
        {
            Console.WriteLine(Message);
        }

        private void WriteLogFile(string Message)
        {
            lock (_syncObject)
            {
                {
                    using (System.IO.StreamWriter w = System.IO.File.AppendText(this._fileName))
                    {
                        var now = DateTime.Now;
                        w.WriteLine($"{now.ToShortDateString()} {now.ToShortTimeString()} {Message}");
                        w.Flush();
                    }
                }
            }
        }
    }
}
