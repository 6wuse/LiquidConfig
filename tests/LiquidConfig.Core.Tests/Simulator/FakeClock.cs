using LiquidConfig.Core.Abstractions;

namespace LiquidConfig.Core.Tests.Simulator
{
    /// <summary>可控时钟：让依赖时间的测试不需要真的等待。</summary>
    public sealed class FakeClock : IClock
    {
        public FakeClock(DateTime? start = null)
        {
            UtcNow = start ?? new DateTime(2026, 11, 12, 0, 0, 0, DateTimeKind.Utc);
        }

        public DateTime UtcNow { get; set; }

        public void Advance(TimeSpan delta) => UtcNow += delta;
    }
}
