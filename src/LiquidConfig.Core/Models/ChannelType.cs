//定义了三个通道，温度、压力、流量
namespace LiquidConfig.Core.Models
{
    public enum ChannelType : ushort//表示底层按照2字节符合整数存储
    { Temperature = 0, //温度
        Pressure = 1, //压力
        Flow = 2 //流量
    }
}
