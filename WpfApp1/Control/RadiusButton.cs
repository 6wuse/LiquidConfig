using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace LiquidConfig.App.Control
{
    public class RadiusButton:Button
    {
        public static readonly DependencyProperty DefaultBackgroundProperty =
           DependencyProperty.Register(
                nameof(DefaultBackground),
                typeof(Brush),
                typeof(RadiusButton),
                new PropertyMetadata(default(Brush)));
        public Brush DefaultBackground
        {
            get => (Brush) GetValue(DefaultBackgroundProperty);
            set => SetValue(DefaultBackgroundProperty, value);
        }
        public static readonly DependencyProperty HoverBackgroundProperty =
         DependencyProperty.Register(
              nameof(HoverBackground),
              typeof(Brush),
              typeof(RadiusButton),
              new PropertyMetadata(default(Brush)));
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }
        //属性名字HoverBackground需要和HoverBackgroundProperty相对应才可以

        //鼠标按下颜色
        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register(
                nameof(PressedBackground),
                typeof(Brush),
                typeof(RadiusButton),
                new PropertyMetadata(default(Brush)));
        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(RadiusButton),
                new PropertyMetadata(default(CornerRadius)));
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
