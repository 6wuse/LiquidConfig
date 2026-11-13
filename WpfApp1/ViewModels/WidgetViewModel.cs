using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;

namespace LiquidConfig.App.ViewModels
{
    public partial class WidgetViewModel:ObservableObject
    {
        [ObservableProperty]
        private string _id = "";

        [ObservableProperty]
        private string _title = "控件";

        [ObservableProperty]
        private double _x ;

        [ObservableProperty]
        private double _y;

        [ObservableProperty]
        private double _w = 180;

        [ObservableProperty]
        private double _h = 120;

        [ObservableProperty]
        private Brush _fill = Brushes.SteelBlue;
    }
}
