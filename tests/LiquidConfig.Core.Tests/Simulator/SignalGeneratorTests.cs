using LiquidConfig.Core.Models;
using LiquidConfig.Core.Simulator;

namespace LiquidConfig.Core.Tests.Simulator
{
    public sealed class SignalGeneratorTests
    {
        /// <summary>造一个可控通道配置，噪声默认关掉，便于断言精确值。</summary>
        private static SimChannelConfig Make(
            SimWaveform waveform,
            float baseValue,
            float amplitude = 0f,
            float periodSec = 0f,
            float slope = 0f,
            float phase = 0f,
            float noiseRatio = 0f,
            int seed = 7,
            float rangeLow = 0f,
            float rangeHigh = 100f,
            float alarmLow = 5f,
            float alarmHigh = 90f)
        {
            var desc = new ChannelDesc(0, ChannelType.Temperature, true, "℃",
                rangeLow, rangeHigh, alarmLow, alarmHigh, 200);

            return new SimChannelConfig(desc, waveform, baseValue, amplitude, periodSec, slope, phase, noiseRatio, seed);
        }

        [Fact]
        public void Static_ShouldHoldBaseValue_RegardlessOfTime()
        {
            var cfg = Make(SimWaveform.Static, 12.5f);

            Assert.Equal(12.5f, SignalGenerator.Sample(cfg, 0d));
            Assert.Equal(12.5f, SignalGenerator.Sample(cfg, 123.4d));
        }

        [Fact]
        public void Sine_AtQuarterPeriod_ShouldReachPeak()
        {
            var cfg = Make(SimWaveform.Sine, baseValue: 45f, amplitude: 8f, periodSec: 60f);

            Assert.Equal(45f, SignalGenerator.Sample(cfg, 0d));
            Assert.Equal(53f, SignalGenerator.Sample(cfg, 15d));   // T/4 → 峰值
            Assert.Equal(45f, SignalGenerator.Sample(cfg, 30d));   // T/2 → 回基线
        }

        [Fact]
        public void Sine_WithPhase_ShouldShiftWaveform()
        {
            var noPhase = Make(SimWaveform.Sine, 45f, 8f, 60f);
            var quarter = Make(SimWaveform.Sine, 45f, 8f, 60f, phase: MathF.PI / 2f);

            Assert.Equal(45f, SignalGenerator.Sample(noPhase, 0d));
            Assert.Equal(53f, SignalGenerator.Sample(quarter, 0d));
        }

        [Fact]
        public void Sine_ShouldStayInsideRange()
        {
            var cfg = Make(SimWaveform.Sine, baseValue: 50f, amplitude: 60f, periodSec: 10f, rangeLow: 0f, rangeHigh: 100f);

            for (double t = 0; t < 30; t += 0.17)
            {
                float v = SignalGenerator.Sample(cfg, t);
                Assert.InRange(v, 0f, 100f);
            }
        }

        [Fact]
        public void Ramp_ShouldRiseThenFoldBack()
        {
            // 量程 100~600 是压力通道的写法，这里换个量程验证折返逻辑
            var cfg = Make(SimWaveform.Ramp, baseValue: 0f, slope: 5f, rangeLow: 0f, rangeHigh: 100f);

            Assert.Equal(0f, SignalGenerator.Sample(cfg, 0d));
            Assert.Equal(50f, SignalGenerator.Sample(cfg, 10d));    // 10 s × 5 = 50
            Assert.Equal(100f, SignalGenerator.Sample(cfg, 20d));   // 撞上限

            // 一个完整周期 = 2 × span / slope = 40 s；30 s 时已折返到 50
            Assert.Equal(50f, SignalGenerator.Sample(cfg, 30d));
            Assert.Equal(0f, SignalGenerator.Sample(cfg, 40d));
        }

        [Fact]
        public void Ramp_ShouldNeverLeaveRange()
        {
            var cfg = Make(SimWaveform.Ramp, baseValue: 100f, slope: 5f, rangeLow: 0f, rangeHigh: 1000f);

            for (double t = 0; t < 400; t += 1.3)
            {
                Assert.InRange(SignalGenerator.Sample(cfg, t), 0f, 1000f);
            }
        }

        [Fact]
        public void Step_ShouldToggleEveryHalfPeriod()
        {
            var cfg = Make(SimWaveform.Step, baseValue: 40f, amplitude: 20f, periodSec: 10f);

            Assert.Equal(40f, SignalGenerator.Sample(cfg, 0d));
            Assert.Equal(40f, SignalGenerator.Sample(cfg, 4d));
            Assert.Equal(60f, SignalGenerator.Sample(cfg, 6d));
            Assert.Equal(40f, SignalGenerator.Sample(cfg, 11d));
        }

        [Fact]
        public void FaultWaveform_ShouldExceedRangeHigh()
        {
            var cfg = Make(SimWaveform.Fault, baseValue: 45f, rangeLow: 0f, rangeHigh: 120f, alarmHigh: 85f);

            float v = SignalGenerator.Sample(cfg, 0d);
            Assert.True(v > 120f, $"Fault 波形应超出量程上限，实际 {v}");
            Assert.True(v > 85f, "Fault 波形应触发高报");
        }

        [Fact]
        public void Sample_ShouldBeDeterministic_ForSameSeedAndTime()
        {
            var a = Make(SimWaveform.Sine, 45f, 8f, 60f, noiseRatio: 0.005f, seed: 1000);
            var b = a with { };

            Assert.Equal(SignalGenerator.Sample(a, 12.345d), SignalGenerator.Sample(b, 12.345d));
        }

        [Fact]
        public void Noise_ShouldDiffer_ForDifferentSeeds()
        {
            var a = Make(SimWaveform.Static, 50f, noiseRatio: 0.05f, seed: 11);
            var b = Make(SimWaveform.Static, 50f, noiseRatio: 0.05f, seed: 12);

            // 采样 100 个点，至少有一个点因种子不同而不同（否则说明噪声没起作用）
            bool anyDifferent = false;
            for (int i = 0; i < 100 && !anyDifferent; i++)
            {
                double t = i * 0.2;
                anyDifferent = SignalGenerator.Sample(a, t) != SignalGenerator.Sample(b, t);
            }

            Assert.True(anyDifferent, "不同种子应产生不同噪声序列");
        }

        [Theory]
        [InlineData(12.34f, 12.3f)]
        [InlineData(12.36f, 12.4f)]
        [InlineData(-0.06f, -0.1f)]
        [InlineData(0f, 0f)]
        public void Quantize_ShouldRoundToTenth(float input, float expected)
        {
            Assert.Equal(expected, SignalGenerator.Quantize(input));
        }
    }
}
