
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;  
using Avalonia.Controls.Shapes;
using Avalonia.Input; 
using Avalonia.Markup.Xaml;
using Avalonia.Media; 
using System;
using System.Runtime.InteropServices; 
using System.Threading.Tasks;

namespace WoWmapperX.AvaloniaViews
{
    // https://github.com/FrankenApps/Avalonia-CustomTitleBarTemplate/tree/non-resizable

    public partial class TitleBar : UserControl
    {
        public DockPanel MainBackground { get { return TitleBarBackground; } }

        public static readonly StyledProperty<bool> IsSeamlessProperty =
        AvaloniaProperty.Register<TitleBar, bool>(nameof(IsSeamless));

        public Func<int> SettingsButtonPressed { get; set; }

        private bool isPointerPressed = false;
        private PixelPoint startPosition = new PixelPoint(0, 0);
        private Point mouseOffsetToOrigin = new Point(0, 0);

        public bool IsSeamless
        {
            get { return GetValue(IsSeamlessProperty); }
            set
            {
                SetValue(IsSeamlessProperty, value);
                if (TitleBarBackground != null &&
                    SystemChromeTitle != null &&
                    SeamlessMenuBar != null &&
                    DefaultMenuBar != null)
                {
                    TitleBarBackground.IsVisible = IsSeamless ? false : true;
                    SystemChromeTitle.IsVisible = IsSeamless ? false : true;
                    SeamlessMenuBar.IsVisible = IsSeamless ? true : false;
                    DefaultMenuBar.IsVisible = IsSeamless ? false : true;

                    if (IsSeamless == false)
                    {
                        titleBar.Resources["SystemControlForegroundBaseHighBrush"] = new SolidColorBrush { Color = new Color(255, 0, 0, 0) };
                    }
                }
            }
        }

        public TitleBar()
        {
            this.InitializeComponent();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
            {
                this.IsVisible = false;
            }
            else
            {
                MinimizeButton.Click += MinimizeWindow;
                MaximizeButton.Click += MaximizeWindow;
                CloseButton.Click += CloseWindow;
                //WindowIcon.DoubleTapped += CloseWindow;


                this.PointerPressed += BeginListenForDrag;
                this.PointerMoved += HandlePotentialDrag;
                this.PointerReleased += HandlePotentialDrop;
                this.Background = Brushes.Transparent;


                SubscribeToWindowState();
            }
        }

        private void HandlePotentialDrop(object sender, PointerReleasedEventArgs e)
        {
            var pos = e.GetPosition(this);
            startPosition = new PixelPoint((int)(startPosition.X + pos.X - mouseOffsetToOrigin.X), (int)(startPosition.Y + pos.Y - mouseOffsetToOrigin.Y));
            ((Window)this.VisualRoot).Position = startPosition;
            isPointerPressed = false;
        }

        private void HandlePotentialDrag(object sender, PointerEventArgs e)
        {
            if (isPointerPressed)
            {
                var pos = e.GetPosition(this);
                startPosition = new PixelPoint((int)(startPosition.X + pos.X - mouseOffsetToOrigin.X), (int)(startPosition.Y + pos.Y - mouseOffsetToOrigin.Y));
                ((Window)this.VisualRoot).Position = startPosition;
            }
        }

        private void BeginListenForDrag(object sender, PointerPressedEventArgs e)
        {
            startPosition = ((Window)this.VisualRoot).Position;
            mouseOffsetToOrigin = e.GetPosition(this);
            isPointerPressed = true;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            hostWindow.Close();

        }


        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;

            if (hostWindow.WindowState == WindowState.Normal)
            {
                hostWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                hostWindow.WindowState = WindowState.Normal;
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            hostWindow.WindowState = WindowState.Minimized;
        }

        private async void SubscribeToWindowState()
        {
            Window hostWindow = (Window)this.VisualRoot;

            while (hostWindow == null)
            {
                hostWindow = (Window)this.VisualRoot;
                await Task.Delay(50);
            }

            hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
            {
                if (s != WindowState.Maximized)
                {
                    MaximizeIcon.Data = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                    hostWindow.Padding = new Thickness(0, 0, 0, 0);
                    MaximizeToolTip.Content = "Maximize";
                }
                if (s == WindowState.Maximized)
                {
                    MaximizeIcon.Data = Geometry.Parse("M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
                    hostWindow.Padding = new Thickness(7, 7, 7, 7);
                    MaximizeToolTip.Content = "Restore Down";

                    // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
                    /*hostWindow.Padding = new Thickness(
                            hostWindow.OffScreenMargin.Left,
                            hostWindow.OffScreenMargin.Top,
                            hostWindow.OffScreenMargin.Right,
                            hostWindow.OffScreenMargin.Bottom);*/
                }
            });
        }

        private void Settings_Pressed(object sender, PointerPressedEventArgs e)
        {
            if(e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (SettingsButtonPressed != null)
                    _ = SettingsButtonPressed();
            }
        }
    }
}
