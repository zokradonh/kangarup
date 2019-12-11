using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kangarup.test
{
    class DebugLogger : ILogger
    {
        public void Info(string message)
        {
            Debug.WriteLine(message);
        }

        public void Warn(string message, Exception e = null)
        {
            Debug.WriteLine(message);
            if (e != null) Debug.WriteLine(e.StackTrace);
        }

        public void Error(string message, Exception e = null)
        {
            Debug.WriteLine(message);
            if (e != null) Debug.WriteLine(e.StackTrace);
        }
    }
}
