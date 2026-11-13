using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidConfig.Core.Models
{
    public sealed record ChannelSnapshot(
        DateTime Time,
        IReadOnlyList<TagVariable> Values);
}
