using LiquidConfig.Core.Models;
using LiquidConfig.Core.Simulator;

namespace LiquidConfig.Core.Tests.Simulator
{
    public sealed class SimulatorTagSourceTests
    {
        private static SimulatorTagSource CreateSource(
            out FakeClock clock,
            out FaultInjector injector,
            int channelCount = 4)
        {
            clock = new FakeClock();
            injector = new FaultInjector { TimeoutDelay = TimeSpan.FromMilliseconds(50) };

            return new SimulatorTagSource(
                SimChannelConfigDefaults.CreateDefaultTable(channelCount),
                clock,
                null,
                injector);
        }

        [Fact]
        public async Task OpenAsync_ShouldReturnDeviceInfo_WithChannelCount()
        {
            await using var source = CreateSource(out _, out _);

            var info = await source.OpenAsync(CancellationToken.None);

            Assert.Equal((ushort)4, info.ChannelCount);
            Assert.Equal((ushort)1, info.DeviceStates);   // 1 = Running，与固件枚举一致
            Assert.Equal(ConnectionState.Online, source.State);
        }

        [Fact]
        public async Task OpenAsync_ShouldRaiseStateChanged_ConnectingThenOnline()
        {
            await using var source = CreateSource(out _, out _);

            var states = new List<ConnectionState>();
            source.StateChanged += (_, s) => states.Add(s);

            await source.OpenAsync(CancellationToken.None);

            Assert.Equal(new[] { ConnectionState.Connecting, ConnectionState.Online }, states);
        }

        [Fact]
        public async Task ReadChannelTable_ShouldReturnAscendingIds()
        {
            await using var source = CreateSource(out _, out _);
            await source.OpenAsync(CancellationToken.None);

            var table = await source.ReadChannelTableAsync(CancellationToken.None);

            Assert.Equal(4, table.Count);
            Assert.Equal(new[] { 0, 1, 2, 3 }, table.Select(d => d.Id).ToArray());
            Assert.All(table, d => Assert.True(d.RangeHigh > d.RangeLow));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(32)]
        public async Task ChannelCount_ShouldBeConfigurable(int channelCount)
        {
            await using var source = CreateSource(out _, out _, channelCount);
            await source.OpenAsync(CancellationToken.None);

            var table = await source.ReadChannelTableAsync(CancellationToken.None);
            var snapshot = await source.ReadRealtimeAsync(CancellationToken.None);

            Assert.Equal(channelCount, table.Count);
            Assert.Equal(channelCount, snapshot.Values.Count);
            Assert.Equal(table.OrderBy(d => d.Id).Select(d => d.Id),
                         snapshot.Values.Select(v => v.Id).OrderBy(id => id));
        }

        [Fact]
        public async Task ReadRealtime_BeforeOpen_ShouldThrow()
        {
            await using var source = new SimulatorTagSource(
                SimChannelConfigDefaults.CreateDefaultTable(), new FakeClock());

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => source.ReadRealtimeAsync(CancellationToken.None));
        }

        [Fact]
        public async Task ReadRealtime_ShouldMatchChannelTable()
        {
            await using var source = CreateSource(out FakeClock clock, out _);
            await source.OpenAsync(CancellationToken.None);

            var table = await source.ReadChannelTableAsync(CancellationToken.None);
            clock.Advance(TimeSpan.FromSeconds(3));
            var snapshot = await source.ReadRealtimeAsync(CancellationToken.None);

            Assert.Equal(table.Count, snapshot.Values.Count);

            foreach (var tag in snapshot.Values)
            {
                var desc = table.Single(d => d.Id == tag.Id);
                Assert.Equal(desc.Unit, tag.Unit);
                Assert.Equal(desc.Type, tag.Type);
                Assert.Equal(desc.SamplePeriodMs, tag.SamplePeriodMs);
                Assert.Equal(clock.UtcNow, tag.TimestampUtc);
                Assert.InRange(tag.Value, desc.RangeLow, desc.RangeHigh);
            }
        }

        [Fact]
        public async Task InjectedTimeout_ShouldThrowTimeoutException()
        {
            await using var source = CreateSource(out _, out FaultInjector injector);
            await source.OpenAsync(CancellationToken.None);
            injector.Inject(SimFault.Timeout);

            await Assert.ThrowsAsync<TimeoutException>(
                () => source.ReadRealtimeAsync(CancellationToken.None));
        }

        [Fact]
        public async Task InjectedFreeze_ShouldHoldValueAndMarkStale()
        {
            await using var source = CreateSource(out FakeClock clock, out FaultInjector injector);
            await source.OpenAsync(CancellationToken.None);

            var first = await source.ReadRealtimeAsync(CancellationToken.None);
            injector.Inject(SimFault.Freeze);

            clock.Advance(TimeSpan.FromSeconds(20));
            var second = await source.ReadRealtimeAsync(CancellationToken.None);

            // 值必须冻住（不清零、不继续变化），质量位置 Stale
            Assert.Equal(first.Values[0].Value, second.Values[0].Value);
            Assert.All(second.Values, v => Assert.Equal(TagQuality.Stale, v.Quality));
        }

        [Fact]
        public async Task InjectedOverRange_ShouldExceedRangeAndRaiseAlarmHigh()
        {
            await using var source = CreateSource(out _, out FaultInjector injector);
            await source.OpenAsync(CancellationToken.None);
            injector.Inject(SimFault.OverRange);

            var snapshot = await source.ReadRealtimeAsync(CancellationToken.None);

            Assert.All(snapshot.Values, v =>
            {
                Assert.True(v.Value > v.RangeHigh, $"通道 {v.Id} 值 {v.Value} 应超出量程上限 {v.RangeHigh}");
                Assert.Equal(TagQuality.AlarmHigh, v.Quality);
            });
        }

        [Fact]
        public async Task InjectedBadCrc_ShouldMarkAllInvalid_AndKeepLastValue()
        {
            await using var source = CreateSource(out _, out FaultInjector injector);
            await source.OpenAsync(CancellationToken.None);

            var good = await source.ReadRealtimeAsync(CancellationToken.None);
            injector.Inject(SimFault.BadCrc);

            var bad = await source.ReadRealtimeAsync(CancellationToken.None);

            Assert.All(bad.Values, v => Assert.Equal(TagQuality.Invalid, v.Quality));
            Assert.Equal(good.Values[0].Value, bad.Values[0].Value);
        }

        [Fact]
        public async Task InjectToSingleChannel_ShouldNotAffectOthers()
        {
            await using var source = CreateSource(out _, out FaultInjector injector);
            await source.OpenAsync(CancellationToken.None);
            injector.Inject(SimFault.OverRange, channelId: 0);

            var snapshot = await source.ReadRealtimeAsync(CancellationToken.None);

            Assert.Equal(TagQuality.AlarmHigh, snapshot.Values.Single(v => v.Id == 0).Quality);
            Assert.All(snapshot.Values.Where(v => v.Id != 0),
                v => Assert.NotEqual(TagQuality.AlarmHigh, v.Quality));
        }

        [Fact]
        public async Task WriteChannel_ShouldUpdateDescription()
        {
            await using var source = CreateSource(out _, out _);
            await source.OpenAsync(CancellationToken.None);

            var table = await source.ReadChannelTableAsync(CancellationToken.None);
            var target = table.Single(d => d.Id == 0);
            var newDesc = target with { AlarmHigh = 70f, RangeHigh = 150f };

            await source.WriteChannelAsync(newDesc, CancellationToken.None);

            var after = await source.ReadChannelTableAsync(CancellationToken.None);
            var updated = after.Single(d => d.Id == 0);

            Assert.Equal(70f, updated.AlarmHigh);
            Assert.Equal(150f, updated.RangeHigh);
            // 未被修改的字段必须保持不变，避免"下发参数顺手改了别的"
            Assert.Equal(target.AlarmLow, updated.AlarmLow);
            Assert.Equal(target.Unit, updated.Unit);
        }

        [Fact]
        public async Task WriteChannel_UnknownId_ShouldThrow()
        {
            await using var source = CreateSource(out _, out _);
            await source.OpenAsync(CancellationToken.None);

            var ghost = new ChannelDesc(99, ChannelType.Temperature, true, "℃", 0f, 100f, 5f, 90f, 200);

            await Assert.ThrowsAsync<ArgumentException>(
                () => source.WriteChannelAsync(ghost, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteCommand_ClearAlarm_ShouldResetFaultInjection()
        {
            await using var source = CreateSource(out _, out FaultInjector injector);
            await source.OpenAsync(CancellationToken.None);
            injector.Inject(SimFault.OverRange);

            ushort ack = await source.ExecuteCommandAsync(0x0001, CancellationToken.None);

            Assert.Equal((ushort)0, ack);
            Assert.Equal(SimFault.None, injector.Current);
        }

        [Fact]
        public async Task ExecuteCommand_UnknownCode_ShouldReturnErrorCode()
        {
            await using var source = CreateSource(out _, out _);
            await source.OpenAsync(CancellationToken.None);

            ushort ack = await source.ExecuteCommandAsync(0x00FF, CancellationToken.None);

            Assert.Equal((ushort)0x0001, ack);   // 与 Modbus "非法功能码" 语义对齐
        }

        [Fact]
        public async Task DisposeAsync_ShouldReturnIdle_AndRejectFurtherReads()
        {
            var source = CreateSource(out _, out _);
            await source.OpenAsync(CancellationToken.None);

            await source.DisposeAsync();

            Assert.Equal(ConnectionState.Idle, source.State);
            await Assert.ThrowsAsync<ObjectDisposedException>(
                () => source.ReadRealtimeAsync(CancellationToken.None));
        }

        [Fact]
        public async Task Cancellation_ShouldPropagateOnRead()
        {
            await using var source = CreateSource(out _, out _);
            await source.OpenAsync(CancellationToken.None);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => source.ReadRealtimeAsync(cts.Token));
        }
    }
}
