//定义数据质量
namespace LiquidConfig.Core.Models
{
    public class TagQyality
    {
        public enum TagQuality : ushort
        {
            Good = 0, //数据正常
            AlarmHigh = 1, //数据超过上限
            AlarmLow = 2, //数据低于下限
            CommLost = 3, //通信断开
            Invalid = 4, //数据无效
            Stale = 5 //数据过期
        }
    }
}
