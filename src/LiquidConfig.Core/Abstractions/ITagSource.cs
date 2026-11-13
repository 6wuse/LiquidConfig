using LiquidConfig.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidConfig.Core.Abstractions
{
    //数据源契约，
    //继承IAsyncDisposable ：实现类中必须会异步清理
    public interface ITagSource : IAsyncDisposable
    {
        ConnectionState State { get; }
        //当前连接状态，Idle，Connecting，Onlie，Reconnecting，Fault
        Task<DeviceInfo> OpenAsync(CancellationToken token);
        //读取设备信息，设备ID，固件版本，总通道数，设备状态，当前仿真模式，运行秒数
        Task<IReadOnlyList<ChannelDesc>> ReadChannelTableAsync(CancellationToken token);
        //读取通道描述表，上位机依据此自动创建变量池（自适应机制的唯一来源）
        Task<ChannelSnapshot> ReadRealtimeAsync(CancellationToken token);
        //读一批通道实时值（一次轮询的结果）
        Task WriteChannelAsync(ChannelDesc desc, CancellationToken token);
        //参数下发+回读校验
        Task<ushort> ExecuteCommandAsync(ushort code, CancellationToken token);
        //控制区命令（清除报警，返回应答码）
        event EventHandler<ConnectionState>? StateChanged;
        //状态变化时触发

        //C# 自带的事件（event）机制，带有(object? sender, TEventArgs e)
        //sender：是谁触发的这个事件（发布者，一般填 this）
        //e：自定义参数，这里就是 ConnectionState（连接状态）

        //如果使用Action，没有sender
    }
}


//Task 返回类型 = 异步方法
