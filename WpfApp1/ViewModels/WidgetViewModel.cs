using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;
using System.ComponentModel.DataAnnotations;
using LiquidConfig.App.Services.Behaviors;

namespace LiquidConfig.App.ViewModels
{
    public partial class WidgetViewModel: ObservableValidator,IDraggable
    {
        public const double MinSize = 20;

        [ObservableProperty]
        private string _id = "";

        [ObservableProperty]
        private string _title = "控件";

        [ObservableProperty]
        private Brush _fill = Brushes.SteelBlue;


        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(-10000, 10000, ErrorMessage = "")]
        private double _x;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(-10000, 10000, ErrorMessage = "")]
        private double _y;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(0.01, 10000, ErrorMessage = "")]
        private double _w = 120;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(0.01, 10000, ErrorMessage = "")]
        private double _h = 20;
    }
}
