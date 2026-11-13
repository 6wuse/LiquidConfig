using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidConfig.App.Services.Behaviors
{
    public interface IDraggable
    {
        /// <summary>
        /// 组件可拖动，这是一个能力
        /// </summary>
        double X { get; set; }
        double Y { get; set; }
    }
}
