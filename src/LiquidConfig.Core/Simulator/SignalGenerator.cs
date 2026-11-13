namespace LiquidConfig.Core.Simulator
{
    /// <summary>
    /// 信号发生器：给定「通道配置 + 时间」，算出该时刻的工程量（℃ / kPa / L/min）。
    ///
    /// 硬性要求（会被单元测试卡住）：
    ///   1) Sample 必须是**纯函数**：不持有状态，同样的 (cfg, t) 永远得到同样的结果；
    ///   2) 噪声不能依赖"调用了几次"，要用 (Seed, 采样序号) 推出来；
    ///   3) 除 Fault 波形外，结果必须落在 [RangeLow, RangeHigh] 内，并量化到 1 位小数；
    ///   4) Fault 波形必须超出 RangeHigh（否则永远报不出警）。
    /// </summary>
    public static class SignalGenerator
    {
        /// <summary>量化保留的小数位数（模拟 ADC 离散化，1 位足够真实）。</summary>
        public const int Decimals = 1;

        /// <summary>量化步长（= 10^-Decimals），仅用于外部展示。</summary>
        public const float QuantizeStep = 0.1f;

        /// <summary>按经过秒数取值。</summary>
        public static float Sample(SimChannelConfig cfg, double elapsedSeconds)
        {
            // TODO(练习 3)：按 Waveform 分派到各波形函数，叠加噪声，限幅，最后量化。
            throw new NotImplementedException();
        }

        /// <summary>按 TimeSpan 取值（这一行已经给好，照抄思路即可）。</summary>
        public static float Sample(SimChannelConfig cfg, TimeSpan elapsed) => Sample(cfg, elapsed.TotalSeconds);

        /// <summary>
        /// 量化：四舍五入到 Decimals 位。
        /// 注意别写成 (round(v / 0.1f) * 0.1f)，那会得到 12.400001 这种"看着是 12.4、比着不等于 12.4"的值。
        /// </summary>
        public static float Quantize(float value)
        {
            // TODO(练习 2)：先做这个，最简单，也最容易被浮点坑到。
            throw new NotImplementedException();
        }
    }
}
