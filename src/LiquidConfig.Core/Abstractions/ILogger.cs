using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidConfig.Core.Abstractions
{
    public interface ILogger
    {
        void Info(string tag, string msg);//普通信息
        void Warn(string tag, string msg);//警告
        void Error(string tag, string msg, Exception? ex = null);//错误，可附带异常
    }
}
