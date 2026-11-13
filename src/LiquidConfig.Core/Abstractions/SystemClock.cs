namespace LiquidConfig.Core.Abstractions
{
    /// <summary>
    /// 系统时钟：<see cref="IClock"/> 的默认实现。
    /// 之所以把"取当前时间"抽象成接口，是为了让仿真源、轮询调度、归档服务在单元测试里
    /// 可以注入 <c>FakeClock</c> 精确控制时间，而不必真的 <c>Task.Delay</c>（那会让测试变慢且不稳定）。
    /// </summary>
    public sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
