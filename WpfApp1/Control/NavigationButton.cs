using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace LiquidConfig.App.Control
{
    //自定义控件NavigationButton，专门用再导航界面中（存在互斥效果）
    public class NavigationButton: RadioButton
    {
        public static readonly DependencyProperty DefaultBackgroundProperty
            = DependencyProperty.Register(
                nameof(DefaultBackground), //依赖属性的名字，binding DefaultBackground
                typeof(Brush), //属性的类型
                typeof(NavigationButton), //控件类型
                new PropertyMetadata(default(Brush)));//默认值
        public Brush DefaultBackground
        {
            get => (Brush)GetValue(DefaultBackgroundProperty); 
            set => SetValue(DefaultBackgroundProperty, value);
            //正常来说这里应该读取/写入字段_defaultBackground
            //但是在依赖属性中值是由 WPF属性系统管理的
            ////所以使用GetValue和SetValue，从WPF属性中取值/写值
        }
        public static readonly DependencyProperty HoverBackgroundProperty
                = DependencyProperty.Register(
                   nameof(HoverBackground),
                   typeof(Brush),
                   typeof(NavigationButton),
                   new PropertyMetadata(default(Brush)));
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(NavigationButton),
                new PropertyMetadata(default(CornerRadius)));
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

    }
}
