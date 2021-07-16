using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using HandyControl.Data;
using HandyControl.Tools.Interop;

namespace HandyControl.Controls
{
    public static class WindowAttach
    {
        public static readonly DependencyProperty IsDragElementProperty = DependencyProperty.RegisterAttached(
            "IsDragElement", typeof(bool), typeof(WindowAttach), new PropertyMetadata(ValueBoxes.FalseBox, OnIsDragElementChanged));

        public static void SetIsDragElement(DependencyObject element, bool value)
            => element.SetValue(IsDragElementProperty, ValueBoxes.BooleanBox(value));

        public static bool GetIsDragElement(DependencyObject element)
            => (bool) element.GetValue(IsDragElementProperty);

        private static void OnIsDragElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement ctl)
            {
                if ((bool) e.NewValue)
                {
                    ctl.MouseLeftButtonDown += DragElement_MouseLeftButtonDown;
                }
                else
                {
                    ctl.MouseLeftButtonDown -= DragElement_MouseLeftButtonDown;
                }
            }
        }

        private static void DragElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DependencyObject obj && e.ButtonState == MouseButtonState.Pressed)
            {
                System.Windows.Window.GetWindow(obj)?.DragMove();
            }
        }

        public static readonly DependencyProperty IgnoreAltF4Property = DependencyProperty.RegisterAttached(
            "IgnoreAltF4", typeof(bool), typeof(WindowAttach), new PropertyMetadata(ValueBoxes.FalseBox, OnIgnoreAltF4Changed));

        private static void OnIgnoreAltF4Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Window window)
            {
                if ((bool) e.NewValue)
                {
                    window.PreviewKeyDown += Window_PreviewKeyDown;
                }
                else
                {
                    window.PreviewKeyDown -= Window_PreviewKeyDown;
                }
            }
        }

        private static void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }
        }

        public static void SetIgnoreAltF4(DependencyObject element, bool value)
            => element.SetValue(IgnoreAltF4Property, ValueBoxes.BooleanBox(value));

        public static bool GetIgnoreAltF4(DependencyObject element)
            => (bool) element.GetValue(IgnoreAltF4Property);

        public static readonly DependencyProperty ShowInTaskManagerProperty = DependencyProperty.RegisterAttached(
            "ShowInTaskManager", typeof(bool), typeof(WindowAttach), new PropertyMetadata(ValueBoxes.TrueBox, OnShowInTaskManagerChanged));

        private static void OnShowInTaskManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Window window)
            {
                var v = (bool) e.NewValue;
                window.SetCurrentValue(System.Windows.Window.ShowInTaskbarProperty, v);

                if (v)
                {
                    window.SourceInitialized -= Window_SourceInitialized;
                }
                else
                {
                    window.SourceInitialized += Window_SourceInitialized;
                }
            }
        }

        private static void Window_SourceInitialized(object sender, EventArgs e)
        {
            if (sender is System.Windows.Window window)
            {
                var _ = new WindowInteropHelper(window)
                {
                    Owner = InteropMethods.GetDesktopWindow()
                };
            }
        }

        public static void SetShowInTaskManager(DependencyObject element, bool value)
            => element.SetValue(ShowInTaskManagerProperty, ValueBoxes.BooleanBox(value));

        public static bool GetShowInTaskManager(DependencyObject element)
            => (bool) element.GetValue(ShowInTaskManagerProperty);

        public static readonly DependencyProperty HideWhenClosingProperty = DependencyProperty.RegisterAttached(
            "HideWhenClosing", typeof(bool), typeof(WindowAttach), new PropertyMetadata(ValueBoxes.FalseBox, OnHideWhenClosingChanged));

        private static void OnHideWhenClosingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Window window)
            {
                var v = (bool) e.NewValue;
                if (v)
                {
                    window.Closing += Window_Closing;
                }
                else
                {
                    window.Closing -= Window_Closing;
                }
            }
        }

        private static void Window_Closing(object sender, CancelEventArgs e)
        {
            if (sender is System.Windows.Window window)
            {
                window.Hide();
                e.Cancel = true;
            }
        }

        public static void SetHideWhenClosing(DependencyObject element, bool value)
            => element.SetValue(HideWhenClosingProperty, ValueBoxes.BooleanBox(value));

        public static bool GetHideWhenClosing(DependencyObject element)
            => (bool) element.GetValue(HideWhenClosingProperty);

        public static readonly DependencyProperty KeepCenterOnSizeChangedProperty =
            DependencyProperty.RegisterAttached("KeepCenterOnSizeChanged", typeof(bool), typeof(WindowAttach),
                                                new PropertyMetadata(ValueBoxes.FalseBox, KeepCenterOnSizeChangedPropertyChanged));

        [AttachedPropertyBrowsableForType(typeof(System.Windows.Window))]
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetKeepCenterOnSizeChanged(DependencyObject obj) => (bool) obj.GetValue(KeepCenterOnSizeChangedProperty);
        public static void SetKeepCenterOnSizeChanged(DependencyObject obj, bool value) => obj.SetValue(KeepCenterOnSizeChangedProperty, value);

        private static void KeepCenterOnSizeChangedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if ((bool) e.NewValue)
                    window.SizeChanged += Window_SizeChanged;
                else
                    window.SizeChanged -= Window_SizeChanged;
            }

            static void Window_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                var window = (Window) sender;
                if (window.WindowStartupLocation != WindowStartupLocation.Manual &&
                    window.SizeToContent != SizeToContent.Manual)
                {
                    var focusedElement = Keyboard.FocusedElement;

                    if (e.WidthChanged)
                        window.Left += (e.PreviousSize.Width - e.NewSize.Width) / 2;
                    if (e.HeightChanged)
                        window.Top += (e.PreviousSize.Height - e.NewSize.Height) / 2;

                    if (focusedElement != null)
                        Keyboard.Focus(focusedElement);
                }
            }
        }
    }
}
