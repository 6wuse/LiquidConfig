using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidConfig.Core.Abstractions
{
    public interface ILogger
    {
        void Info(string tag, string msg);
        void Warn(string tag, string msg);
        void Error(string tag, string msg, Exception? ex = null);
    }
}
