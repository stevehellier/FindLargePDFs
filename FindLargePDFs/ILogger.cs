using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindLargePDFs
{
    interface ILogger
    {
        void WriteMessage(string Message);
        void AddLogger(Loggers.Logtypes logger);
    }
}
