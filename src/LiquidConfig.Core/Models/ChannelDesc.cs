using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidConfig.Core.Models
{
    //sealed 不允许继承，纯数据，不是基类
    //record （）中的参数是位置参数，编译器会自动生成同名的属性，不用再get，set
    public sealed record ChannelDesc(
        int Id,   //通道编号
        ChannelType Type,//通道类型，温度，流量，压力
        bool Enabled,  //是否启动
        string Unit,   //单位
        float RangeLow, float RangeHigh,//量程上下限
        float AlarmLow, float AlarmHigh,//报警值上下限
        ushort SamplePeriodMs  //采用周期
        );
}
