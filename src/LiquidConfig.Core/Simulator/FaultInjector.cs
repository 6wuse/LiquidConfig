namespace LiquidConfig.Core.Simulator
{
    /// <summary>
    /// 仿真故障类型，对应 T10 要求注入的四种故障。
    /// Timeout（无应答）/ BadCrc（收到但校验失败，整帧丢弃）/ Freeze（值卡死）/ OverRange（超量程）。
    /// </summary>
    public enum SimFault
    {
        None = 0,
        Timeout = 1,
        BadCrc = 2,
        Freeze = 3,
        OverRange = 4
    }

    /// <summary>
    /// 故障注入器：**只记录状态，不负责行为**。
    /// 具体行为由 SimulatorTagSource 在自己的环节读取状态后执行，
    /// 这样注入器和协议/数据源实现解耦，M1 接板卡后同一个注入器还能复用。
    /// </summary>
    public sealed class FaultInjector
    {
        // 提示：状态会被 UI 线程写、被采集线程读，所以所有读写都要在同一把锁里。
        private readonly object _gate = new();

        private SimFault _current = SimFault.None;
        private int? _channelId;

        /// <summary>当前故障类型。</summary>
        public SimFault Current
        {
            get
            {
                // TODO(练习 4)：加锁后返回 _current
                throw new NotImplementedException();
            }
        }

        /// <summary>故障只作用于该通道；null 表示作用于全部通道。</summary>
        public int? ChannelId
        {
            get
            {
                // TODO(练习 4)：加锁后返回 _channelId
                throw new NotImplementedException();
            }
        }

        /// <summary>注入 Timeout 时的模拟等待时长，要大于轮询超时阈值（200 ms）才能真正触发超时。</summary>
        public TimeSpan TimeoutDelay { get; set; } = TimeSpan.FromMilliseconds(400);

        /// <summary>注入故障；channelId 为 null 时作用于全部通道。</summary>
        public void Inject(SimFault fault, int? channelId = null)
        {
            // TODO(练习 4)
            throw new NotImplementedException();
        }

        /// <summary>清除故障，恢复无注入状态。</summary>
        public void Clear()
        {
            // TODO(练习 4)
            throw new NotImplementedException();
        }

        /// <summary>判断某通道当前是否处于注入生效状态（None 时恒为 false；命中指定通道或作用于全部通道时为 true）。</summary>
        public bool IsActiveFor(int channelId)
        {
            // TODO(练习 4)
            throw new NotImplementedException();
        }
    }
}
