using System;

namespace kangarup
{
    public interface ILogger
    {
        void Info(string message);
        void Warn(string message, Exception e = null);
        void Error(string message, Exception e = null);

    }
}