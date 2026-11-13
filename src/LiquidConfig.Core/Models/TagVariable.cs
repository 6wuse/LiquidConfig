using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidConfig.Core.Models
{
    //设备快照(固定时间刷新)
    public sealed record TagVariable(
       int Id,
       ChannelType Type,
       string Unit,
       float RangeLow, float RangeHigh,
       float AlarmLow, float AlarmHigh,
       ushort SamplePeriodMs,
       string Name,
       float Value,
       TagQuality Quality,
       DateTime TimestampUtc
       );
}
