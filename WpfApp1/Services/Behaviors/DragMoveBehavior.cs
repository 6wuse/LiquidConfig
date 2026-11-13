using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LiquidConfig.App.Services.Behaviors
{
    /// <summary>
    /// 控件拖动功能
    /// </summary>
    public class DragMoveBehavior
    {
        public const double GridSize = 8;//网格吸附尺寸

        //附加属性，附加属性可以贴在任意 DependencyObject
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached(
                "Enabled",
                typeof(bool),
                typeof(DragMoveBehavior),
                new PropertyMetadata(false, OnEnabledChanged));//OnEnabledChanged是值变化回调，当在XMAL中把Enabled设置为True时，将调用

        public static bool GetEnabled(DependencyObject obj)
        { 
            return (bool)obj.GetValue(EnabledProperty); 
        }

        public static void SetEnabled(DependencyObject obj, bool value) 
        { 
            obj.SetValue(EnabledProperty, value); 
        }


        private static readonly ConditionalWeakTable<ItemsControl, Session> Sessions = new();
        //ConditionalWeakTable是弱引用字典，键ItemsControl被回收时，值Session也会被回收。这里用它来存储每个 ItemsControl 的拖动状态信息。
        private sealed class Session   //将拖动的状态信息封装在Session类中
        {
            public bool Dragging;//正在拖动
            public ListBoxItem? Container;//拖动的元素容器
            public IDraggable? Target;//容器中的数据对象
            public Panel? Host;//坐标参照物，真正摆放组件的Canvas
            public double OffsetX;//鼠标点击位置相对于容器的偏移量
            public double OffsetY;
        }


        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)//这里d是附加属性的宿主对象ListBox，e是属性值变化信息
        {
            if (d is not ItemsControl host)//判断宿主对象是否是ItemsControl（ListBox、ListView等），如果不是就直接返回，是就赋值给host变量
                return;
            if (Sessions.TryGetValue(host, out _))//去Sessions字典里查找host对应的Session，如果有，就调用Unhook方法解绑事件，并从Sessions字典里移除host。
            {
                Unhook(host);
                Sessions.Remove(host);
            }

            if (Equals(e.NewValue, true))//最新的值为True
            {
                Sessions.Add(host, new Session());//这里Add的时候不允许有重复键，所以上面必须检查是否host（d）是否已经存在
                Hook(host);
            }
        }

        private static void Hook(ItemsControl host)
        {
            host.PreviewMouseLeftButtonDown += OnDown;
            host.PreviewMouseMove += OnMove;
            host.PreviewMouseLeftButtonUp += OnUp;
            host.LostMouseCapture += OnLostCapture;
        }

        private static void Unhook(ItemsControl host)
        {
            host.PreviewMouseLeftButtonDown -= OnDown;
            host.PreviewMouseMove -= OnMove;
            host.PreviewMouseLeftButtonUp -= OnUp;
            host.LostMouseCapture -= OnLostCapture;
        }

        private static void OnDown(object sender, MouseButtonEventArgs e)//按下
        {
            if (sender is not ItemsControl host)//判断宿主对象是否是ItemsControl，如果不是就直接返回，是就赋值给host变量
                return;
            if (!Sessions.TryGetValue(host, out var session))//去Sessions字典里查找host对应的Session，如果没有，就直接返回
                return;
            if (e.LeftButton != MouseButtonState.Pressed)//左键没有按下就直接返回
                return;

            session.Container = FindContainer(e.OriginalSource as DependencyObject);
            //e.OriginalSource是鼠标点击的最底层元素，可能是TextBlock、Border等
            //使用as做一个安全类型转换
            session.Target = session.Container?.DataContext as IDraggable;
            //如果不为null,就将session.Container的DataContext转换为IDraggable类型


            session.Host = session.Container is null ? null : VisualTreeHelper.GetParent(session.Container) as Panel;


            // 没点到组件，点到的组件不是IDraggable,父容器不是Panel
            if ( session.Container is null || session.Target is null || session.Host is null)
            {
                session.Dragging = false;   
                return;
            }
            // 页面被切走/窗口被关闭时这里就是 null，宁可不拖，也别拿它去算坐标。
            if (PresentationSource.FromVisual(session.Host) is null)
            {
                session.Dragging = false;
                return;
            }

            var inContainer = e.GetPosition(session.Container);//当前鼠标点，相对于这个元素左上角的坐标
            session.OffsetX = inContainer.X;
            session.OffsetY = inContainer.Y;
            session.Dragging = true;
            host.CaptureMouse();     // 所有鼠标事件（移动、按下、抬起），不管指针在哪里，都先送到这个 ListBox。
        }

        private static void OnMove(object sender, MouseEventArgs e)//移动
        {
            if (sender is not ItemsControl host)
                return;

            if (!Sessions.TryGetValue(host, out var session) || !session.Dragging)
                return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                session.Dragging = false;
                return;
            }

            if (session.Target is not { } target || session.Host is not { } panel)
                return;

            if (PresentationSource.FromVisual(panel) is null)
            {
                // 画布在拖动过程中被切走：安全结束，不要再去求坐标。
                session.Dragging = false;
                host.ReleaseMouseCapture();
                return;
            }

            var inHost = e.GetPosition(panel);
            target.X = inHost.X - session.OffsetX;
            target.Y = inHost.Y - session.OffsetY;
        }

        private static void OnUp(object sender, MouseButtonEventArgs e)//松开
        {
            if (sender is not ItemsControl host)
                return;

            if (!Sessions.TryGetValue(host, out var session) || !session.Dragging)
                return;

            session.Dragging = false;

            if (host.IsMouseCaptured)
            {
                host.ReleaseMouseCapture();//鼠标松开后，释放接收的鼠标事件指针 ，返回
            }

            if (session.Target is { } target)
            {
                target.X = Snap(target.X);
                target.Y = Snap(target.Y);
            }

            session.Container = null;
            session.Target = null;
        }

        private static void OnLostCapture(object sender, MouseEventArgs e)
        {
            // 只有左键真的松开才算拖动结束。中途被别人抢走捕获（ListBox 的选中逻辑就是这种）不算。
            if (e.LeftButton == MouseButtonState.Pressed)
                return;

            if (sender is ItemsControl host && Sessions.TryGetValue(host, out var session))
                session.Dragging = false;
        }

        private static double Snap(double value)
            => double.IsFinite(value) ? Math.Round(value / GridSize) * GridSize : 0;


        private static ListBoxItem? FindContainer(DependencyObject? source) //从鼠标点击的元素 一直推导其组件ListBoxItem
        {
            while (source is not null)
            {
                if (source is ListBoxItem item) //点击的元素是ListBoxItem，直接返回
                    return item;
                // VisualTreeHelper.GetParent 只吃 Visual；TextBlock/Border 是，个别 ContentElement（如 Run）不是，
                if (source is not Visual)
                    return null;
                source = VisualTreeHelper.GetParent(source);// 往上走一层，开启下一轮循环
            }
            return null;
        }
    }
}
