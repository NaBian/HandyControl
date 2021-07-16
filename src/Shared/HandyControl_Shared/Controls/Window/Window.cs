using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Data;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using HandyControl.Tools.Interop;
#if NET40
using Microsoft.Windows.Shell;
#else
using System.Windows.Shell;
#endif

namespace HandyControl.Controls
{
    [TemplatePart(Name = ElementNonClientArea, Type = typeof(UIElement))]
    public class Window : System.Windows.Window
    {
        private const string ElementNonClientArea = "PART_NonClientArea";

        public static readonly DependencyProperty NonClientAreaContentProperty = DependencyProperty.Register(
            "NonClientAreaContent", typeof(object), typeof(Window), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty CloseButtonHoverBackgroundProperty = DependencyProperty.Register(
            "CloseButtonHoverBackground", typeof(Brush), typeof(Window),
            new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty CloseButtonHoverForegroundProperty =
            DependencyProperty.Register(
                "CloseButtonHoverForeground", typeof(Brush), typeof(Window),
                new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty CloseButtonBackgroundProperty = DependencyProperty.Register(
            "CloseButtonBackground", typeof(Brush), typeof(Window), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty CloseButtonForegroundProperty = DependencyProperty.Register(
            "CloseButtonForeground", typeof(Brush), typeof(Window),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty OtherButtonBackgroundProperty = DependencyProperty.Register(
            "OtherButtonBackground", typeof(Brush), typeof(Window), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty OtherButtonForegroundProperty = DependencyProperty.Register(
            "OtherButtonForeground", typeof(Brush), typeof(Window),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty OtherButtonHoverBackgroundProperty = DependencyProperty.Register(
            "OtherButtonHoverBackground", typeof(Brush), typeof(Window),
            new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty OtherButtonHoverForegroundProperty =
            DependencyProperty.Register(
                "OtherButtonHoverForeground", typeof(Brush), typeof(Window),
                new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty NonClientAreaBackgroundProperty = DependencyProperty.Register(
            "NonClientAreaBackground", typeof(Brush), typeof(Window),
            new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty NonClientAreaForegroundProperty = DependencyProperty.Register(
            "NonClientAreaForeground", typeof(Brush), typeof(Window),
            new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty NonClientAreaHeightProperty = DependencyProperty.Register(
            "NonClientAreaHeight", typeof(double), typeof(Window),
            new PropertyMetadata(22.0));

        public static readonly DependencyProperty ShowNonClientAreaProperty = DependencyProperty.Register(
            "ShowNonClientArea", typeof(bool), typeof(Window),
            new PropertyMetadata(ValueBoxes.TrueBox, OnShowNonClientAreaChanged));

        public static readonly DependencyProperty ShowTitleProperty = DependencyProperty.Register(
            "ShowTitle", typeof(bool), typeof(Window), new PropertyMetadata(ValueBoxes.TrueBox));

        public static readonly DependencyProperty IsFullScreenProperty = DependencyProperty.Register(
            "IsFullScreen", typeof(bool), typeof(Window),
            new PropertyMetadata(ValueBoxes.FalseBox, OnIsFullScreenChanged));

        public static readonly DependencyProperty ExtendViewIntoNonClientAreaProperty = DependencyProperty.Register(
                "ExtendViewIntoNonClientArea", typeof(bool), typeof(Window),
                new PropertyMetadata(ValueBoxes.FalseBox));

        private bool _isFullScreen;

        private Thickness _actualBorderThickness;

        private readonly Thickness _commonPadding;

        private bool _showNonClientArea = true;

        private double _tempNonClientAreaHeight;

        private WindowState _tempWindowState;

        private WindowStyle _tempWindowStyle;

        private ResizeMode _tempResizeMode;

        private UIElement _nonClientArea;

        static Window()
        {
            StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(ResourceHelper.GetResourceInternal<Style>(ResourceToken.WindowWin10)));
        }

        public Window()
        {
#if NET40
            var chrome = new WindowChrome
            {
                CornerRadius = new CornerRadius(),
                GlassFrameThickness = new Thickness(0, 0, 0, 1)
            };
#else
            var chrome = new WindowChrome
            {
                CornerRadius = new CornerRadius(),
                GlassFrameThickness = new Thickness(0, 0, 0, 1),
                UseAeroCaptionButtons = false
            };
#endif
            BindingOperations.SetBinding(chrome, WindowChrome.CaptionHeightProperty,
                new Binding(NonClientAreaHeightProperty.Name) { Source = this });
            WindowChrome.SetWindowChrome(this, chrome);
            _commonPadding = Padding;

            Loaded += (s, e) => OnLoaded(e);
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (InteropValues.MINMAXINFO) Marshal.PtrToStructure(lParam, typeof(InteropValues.MINMAXINFO));
            var monitor = InteropMethods.MonitorFromWindow(hwnd, InteropValues.MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero && mmi != null)
            {
                InteropValues.APPBARDATA appBarData = default;
                var autoHide = InteropMethods.SHAppBarMessage(4, ref appBarData) != 0;
                if (autoHide)
                {
                    var monitorInfo = default(InteropValues.MONITORINFO);
                    monitorInfo.cbSize = (uint) Marshal.SizeOf(typeof(InteropValues.MONITORINFO));
                    InteropMethods.GetMonitorInfo(monitor, ref monitorInfo);
                    var rcWorkArea = monitorInfo.rcWork;
                    var rcMonitorArea = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top - 1);
                }
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            this.GetHwndSource()?.AddHook(HwndSourceHook);
            base.OnSourceInitialized(e);
        }

        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            switch (msg)
            {
                case InteropValues.WM_WINDOWPOSCHANGED:
                    Padding = WindowState == WindowState.Maximized ? WindowHelper.WindowMaximizedPadding : _commonPadding;
                    break;
                case InteropValues.WM_GETMINMAXINFO:
                    WmGetMinMaxInfo(hwnd, lparam);
                    Padding = WindowState == WindowState.Maximized ? WindowHelper.WindowMaximizedPadding : _commonPadding;
                    break;
                case InteropValues.WM_NCHITTEST:
                    // for fixing #886
                    // https://developercommunity.visualstudio.com/t/overflow-exception-in-windowchrome/167357
                    try
                    {
                        _ = lparam.ToInt32();
                    }
                    catch (OverflowException)
                    {
                        handled = true;
                    }
                    break;
            }

            return IntPtr.Zero;
        }


        public Brush CloseButtonBackground
        {
            get => (Brush) GetValue(CloseButtonBackgroundProperty);
            set => SetValue(CloseButtonBackgroundProperty, value);
        }

        public Brush CloseButtonForeground
        {
            get => (Brush) GetValue(CloseButtonForegroundProperty);
            set => SetValue(CloseButtonForegroundProperty, value);
        }

        public Brush OtherButtonBackground
        {
            get => (Brush) GetValue(OtherButtonBackgroundProperty);
            set => SetValue(OtherButtonBackgroundProperty, value);
        }

        public Brush OtherButtonForeground
        {
            get => (Brush) GetValue(OtherButtonForegroundProperty);
            set => SetValue(OtherButtonForegroundProperty, value);
        }

        public double NonClientAreaHeight
        {
            get => (double) GetValue(NonClientAreaHeightProperty);
            set => SetValue(NonClientAreaHeightProperty, value);
        }

        public bool IsFullScreen
        {
            get => (bool) GetValue(IsFullScreenProperty);
            set => SetValue(IsFullScreenProperty, ValueBoxes.BooleanBox(value));
        }

        public object NonClientAreaContent
        {
            get => GetValue(NonClientAreaContentProperty);
            set => SetValue(NonClientAreaContentProperty, value);
        }

        public Brush CloseButtonHoverBackground
        {
            get => (Brush) GetValue(CloseButtonHoverBackgroundProperty);
            set => SetValue(CloseButtonHoverBackgroundProperty, value);
        }

        public Brush CloseButtonHoverForeground
        {
            get => (Brush) GetValue(CloseButtonHoverForegroundProperty);
            set => SetValue(CloseButtonHoverForegroundProperty, value);
        }

        public Brush OtherButtonHoverBackground
        {
            get => (Brush) GetValue(OtherButtonHoverBackgroundProperty);
            set => SetValue(OtherButtonHoverBackgroundProperty, value);
        }

        public Brush OtherButtonHoverForeground
        {
            get => (Brush) GetValue(OtherButtonHoverForegroundProperty);
            set => SetValue(OtherButtonHoverForegroundProperty, value);
        }

        public Brush NonClientAreaBackground
        {
            get => (Brush) GetValue(NonClientAreaBackgroundProperty);
            set => SetValue(NonClientAreaBackgroundProperty, value);
        }

        public Brush NonClientAreaForeground
        {
            get => (Brush) GetValue(NonClientAreaForegroundProperty);
            set => SetValue(NonClientAreaForegroundProperty, value);
        }

        public bool ShowNonClientArea
        {
            get => (bool) GetValue(ShowNonClientAreaProperty);
            set => SetValue(ShowNonClientAreaProperty, ValueBoxes.BooleanBox(value));
        }

        public bool ShowTitle
        {
            get => (bool) GetValue(ShowTitleProperty);
            set => SetValue(ShowTitleProperty, ValueBoxes.BooleanBox(value));
        }

        public bool ExtendViewIntoNonClientArea
        {
            get => (bool) GetValue(ExtendViewIntoNonClientAreaProperty);
            set => SetValue(ExtendViewIntoNonClientAreaProperty, ValueBoxes.BooleanBox(value));
        }
        
        private static void OnShowNonClientAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (Window) d;
            ctl.SwitchShowNonClientArea((bool) e.NewValue);
        }

        private void SwitchShowNonClientArea(bool showNonClientArea)
        {
            if (_nonClientArea == null)
            {
                _showNonClientArea = showNonClientArea;
                return;
            }

            if (showNonClientArea)
            {
                if (IsFullScreen)
                {
                    _nonClientArea.Show(false);
                    _tempNonClientAreaHeight = NonClientAreaHeight;
                    NonClientAreaHeight = 0;
                }
                else
                {
                    _nonClientArea.Show(true);
                    NonClientAreaHeight = _tempNonClientAreaHeight;
                }
            }
            else
            {
                _nonClientArea.Show(false);
                _tempNonClientAreaHeight = NonClientAreaHeight;
                NonClientAreaHeight = 0;
            }
        }

        private static void OnIsFullScreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (Window) d;
            ctl.SwitchIsFullScreen((bool) e.NewValue);
        }

        private void SwitchIsFullScreen(bool isFullScreen)
        {
            if (_nonClientArea == null)
            {
                _isFullScreen = isFullScreen;
                return;
            }

            if (isFullScreen)
            {
                _nonClientArea.Show(false);
                _tempNonClientAreaHeight = NonClientAreaHeight;
                NonClientAreaHeight = 0;

                _tempWindowState = WindowState;
                _tempWindowStyle = WindowStyle;
                _tempResizeMode = ResizeMode;
                WindowStyle = WindowStyle.None;
                //下面三行不能改变，就是故意的
                WindowState = WindowState.Maximized;
                WindowState = WindowState.Minimized;
                WindowState = WindowState.Maximized;
            }
            else
            {
                if (ShowNonClientArea)
                {
                    _nonClientArea.Show(true);
                    NonClientAreaHeight = _tempNonClientAreaHeight;
                }
                else
                {
                    _nonClientArea.Show(false);
                    _tempNonClientAreaHeight = NonClientAreaHeight;
                    NonClientAreaHeight = 0;
                }

                WindowState = _tempWindowState;
                WindowStyle = _tempWindowStyle;
                ResizeMode = _tempResizeMode;
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Maximized)
            {
                BorderThickness = new Thickness();
                _tempNonClientAreaHeight = NonClientAreaHeight;
                NonClientAreaHeight += 8;
            }
            else
            {
                BorderThickness = _actualBorderThickness;
                NonClientAreaHeight = _tempNonClientAreaHeight;
            }
        }

        protected void OnLoaded(RoutedEventArgs args)
        {
            _actualBorderThickness = BorderThickness;
            _tempNonClientAreaHeight = NonClientAreaHeight;

            if (WindowState == WindowState.Maximized)
            {
                BorderThickness = new Thickness();
                _tempNonClientAreaHeight += 8;
            }

            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand,
                (s, e) => WindowState = WindowState.Minimized));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand,
                (s, e) => WindowState = WindowState.Maximized));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand,
                (s, e) => WindowState = WindowState.Normal));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (s, e) => Close()));
            CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu));

            _tempWindowState = WindowState;
            _tempWindowStyle = WindowStyle;
            _tempResizeMode = ResizeMode;

            SwitchIsFullScreen(_isFullScreen);
            SwitchShowNonClientArea(_showNonClientArea);

            if (WindowState == WindowState.Maximized)
            {
                _tempNonClientAreaHeight -= 8;
            }

            if (SizeToContent != SizeToContent.WidthAndHeight)
                return;

            SizeToContent = SizeToContent.Height;
            Dispatcher.BeginInvoke(new Action(() => { SizeToContent = SizeToContent.WidthAndHeight; }));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _nonClientArea = GetTemplateChild(ElementNonClientArea) as UIElement;
        }

        private void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
        {
            var point = WindowState == WindowState.Maximized
                ? new Point(0, NonClientAreaHeight)
                : new Point(Left, Top + NonClientAreaHeight);
            SystemCommands.ShowSystemMenu(this, point);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (SizeToContent == SizeToContent.WidthAndHeight)
                InvalidateMeasure();
        }
    }
}
