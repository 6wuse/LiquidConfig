using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidConfig.Core.Models
{
    public sealed record DeviceInfo(
        ushort DeviceId,//设备ID
        string FirmwareVersion,//固件版本
        ushort ChannelCount,//总通道数
        ushort DeviceStates,//设备状态（0=Init 1=Running 2=Fault）
        ushort SimMode,//当前仿真模式
        uint UptimeSeconds);//运行秒数
}