using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace LiquidConfig.App.Control
{
    public class RadiusRepeatButton : RepeatButton//这个按钮可以实现，长按一直执行Command
    {
        //Button中存在Backgroud，不用在依赖属性中添加
        //还是添加，这样可以为不同共功能的按钮设置不同的颜色，并且还不需要考虑优先级的问题
        //Background="#F8FAFC" HoverBackground="#DCEEFF" PressedBackground="#5B9BD5"
        //一直显示灰色，即Background="#F8FAFC"优先级最高
        //否则必须把DefaultBackground放在样式中，这样就不能轻易更改颜色了。

        public static readonly DependencyProperty DefaultBackgroundProperty =
           DependencyProperty.Register(
                nameof(DefaultBackground),
                typeof(Brush),
                typeof(RadiusRepeatButton),
                new PropertyMetadata(default(Brush)));
        public Brush DefaultBackground
        {
            get => (Brush)GetValue(DefaultBackgroundProperty);
            set => SetValue(DefaultBackgroundProperty, value);
        }

        //鼠标悬停颜色
        public static readonly DependencyProperty HoverBackgroundProperty =
           DependencyProperty.Register(
                nameof(HoverBackground),
                typeof(Brush),
                typeof(RadiusRepeatButton),
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
                typeof(RadiusRepeatButton),
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
                typeof(RadiusRepeatButton),
                new PropertyMetadata(default(CornerRadius)));
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
