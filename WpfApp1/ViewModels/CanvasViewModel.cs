using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.IO;
using System.Text;

namespace LiquidConfig.App.ViewModels
{
    public class CanvasViewModel
    {
        public ObservableCollection<WidgetViewModel> Widgets { get; } = new();

        public CanvasViewModel()
        {
            Widgets.Add(new WidgetViewModel { Id = "W1", Title = "温度℃", X = 100, Y = 80 });
            Widgets.Add(new WidgetViewModel { Id = "W2", Title = "压力Mpa", X = 400, Y = 80 ,Fill = Brushes.Orange });
            Widgets.Add(new WidgetViewModel { Id = "W3", Title = "流量L/min", X = 100, Y = 260,W = 120,H = 120 });
        }
    }
}
