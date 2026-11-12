//定义连接状态枚举
namespace LiquidConfig.Core.Models
{
    public enum ConnectionState
    {
        Idle,//未连接
        Connecting,//正在连接
        Online,//已连接
        Reconnecting,//正在重新连接
        Fault//连接异常
    }
}
