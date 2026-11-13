using LiquidConfig.Core.Models;

namespace LiquidConfig.Core.Simulator
{
    /// <summary>
    /// 仿真波形类型。数值必须与固件控制区 CTRL_SIM_MODE 的取值一致
    /// （0 Static / 1 Sine / 2 Ramp / 3 Step / 4 Fault），这样 M0 与 M1 是同一套语义。
    /// </summary>
    public enum SimWaveform : ushort
    {
        Static = 0,
        Sine = 1,
        Ramp = 2,
        Step = 3,
        Fault = 4
    }

    /// <summary>
    /// 单通道仿真配置：<see cref="Desc"/> 是"语义"（要上报给上位机的公共信息），
    /// 其余字段是"行为"（只属于仿真源内部，不能混进 ChannelDesc，否则 M1 无法实现）。
    /// 这个 record 的字段顺序由测试用例固定，不要调整。
    /// </summary>
    public sealed record SimChannelConfig(
        ChannelDesc Desc,
        SimWaveform Waveform,
        float BaseValue,
        float Amplitude,
        float PeriodSec,
        float SlopePerSec,
        float PhaseRad,
        float NoiseRatio,
        int Seed);

    /// <summary>默认通道表的构造入口（内容见对话里的任务书 §2）。</summary>
    public static class SimChannelConfigDefaults
    {
        /// <summary>默认 4 通道表。</summary>
        public static IReadOnlyList<SimChannelConfig> CreateDefaultTable() => CreateDefaultTable(4);

        /// <summary>
        /// 按通道数生成默认表，支持 1~32。
        /// 超过 4 路时按 4 种模板循环取用，并重新编号（Id 不能重复），相位要错开。
        /// 参数越界时抛 ArgumentOutOfRangeException。
        /// </summary>
        public static IReadOnlyList<SimChannelConfig> CreateDefaultTable(int channelCount)
        {
            // TODO(练习 1)：实现默认通道表。先看任务书里的表格，再动手。
            throw new NotImplementedException();
        }
    }
}
