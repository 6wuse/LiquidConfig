using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LiquidConfig.App.ViewModels
{
    public partial class CanvasViewModel:ObservableObject
    {
        public ObservableCollection<WidgetViewModel> Widgets { get; } = new();

        [ObservableProperty]
        private WidgetViewModel? _selected;

        public CanvasViewModel()
        {

            Widgets.Add(new WidgetViewModel { Id = "W2", Title = "压力Mpa", X = 400, Y = 80 ,Fill = Brushes.Orange });
            Widgets.Add(new WidgetViewModel { Id = "W3", Title = "流量L/min", X = 100, Y = 260});

            Selected = Widgets[0];
        }
    }
}
