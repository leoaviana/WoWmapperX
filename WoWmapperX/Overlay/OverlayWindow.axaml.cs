using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using WoWmapperX.WorldOfWarcraft;
using System;
using System.Threading.Tasks;
using WoWmapperX.AvaloniaImpl;
using WoWmapperX.Native;

namespace WoWmapperX.Overlay
{
    public partial class OverlayWindow : Window
    {
        [LibraryImport("user32.dll")]
        private static partial System.IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom)
            {
            }

            public int X
            {
                get { return Left; }
                set
                {
                    Right -= (Left - value);
                    Left = value;
                }
            }

            public int Y
            {
                get { return Top; }
                set
                {
                    Bottom -= (Top - value);
                    Top = value;
                }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set
                {
                    X = value.X;
                    Y = value.Y;
                }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set
                {
                    Width = value.Width;
                    Height = value.Height;
                }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture,
                    "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        public Image GetImageCrosshair { get => ImageCrosshair; }
        public StackPanel GetStackNotifications { get => StackNotifications; }

        private CancellationTokenSource _cts = new CancellationTokenSource(); 

        public OverlayWindow()
        {

            InitializeComponent();

            if (Design.IsDesignMode) return;

            this.Initialized += OverlayWindow_Initialized;
            SetLayeredWindowAttributes(this.TryGetPlatformHandle().Handle, 0, 0, 0x2); // this.IsVisible = false;
            this.Show();
            Task t = Task.Run(() => ControllerThread(_cts.Token), _cts.Token);
        }

        private void OverlayWindow_Initialized(object sender, EventArgs e)
        {
            NativeMethods.SetWindowExTransparent(this.TryGetPlatformHandle().Handle);
            SetLayeredWindowAttributes(this.TryGetPlatformHandle().Handle, 0, 255, 0x2);
        }

        public void CloseOverlay()
        {
            _cts.Cancel();
            Close();
        }

        public void SetCrosshairState(bool visible, int x = 0, int y = 0)
        {
            try
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var rect = ProcessManager.GetClientRectangle();
                    Canvas.SetLeft(ImageCrosshair, x - ImageCrosshair.Width / 2 - rect.Left);
                    Canvas.SetTop(ImageCrosshair, y - ImageCrosshair.Height / 2 - rect.Top);

                    ImageCrosshair.IsVisible = visible;
                });
            }
            catch
            {
            }
        }

        public void PopupNotification(OverlayNotification notification)
        {
            if (AppSettings.Default.EnableOverlay)
                try
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        if (notification.UniqueID != null &&
                            StackNotifications.Children
                                .Any(
                                    toast => (toast as OverlayToast)?.BaseNotification.UniqueID == notification.UniqueID))
                            return;
                        var popupNotification = new OverlayToast(notification);
                        popupNotification.NotificationCompleted += (sender, args) =>
                        {
                            StackNotifications.Children.Remove(popupNotification);
                            popupNotification = null;
                        };
                        StackNotifications.Children.Add(popupNotification);
                    });
                }
                catch (ThreadAbortException)
                {

                }
                catch (System.Exception ex)
                {
                    Log.WriteLine($"Exception occured during popup creation: {ex.Message}");
                }
        } 

        private System.Drawing.Rectangle _lastRect;


        private void ControllerThread(CancellationToken token)
        {
            while (true)
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    // If the game has focus, attempt to position overlay
                    if (ProcessManager.GameProcess != null && ProcessManager.GameProcess.MainWindowHandle != System.IntPtr.Zero)
                    {
                        var hWndFg = GetForegroundWindow();
                        var rect = ProcessManager.GetClientRectangle();
                        var topMost = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => Topmost);
                        topMost.Wait();

                        // If game window has moved or resized
                        if (rect != _lastRect)
                        {
                            // Update overlay position
                            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                            {
                                Position = new PixelPoint(rect.Left, rect.Top);
                                Width = rect.Width;
                                Height = rect.Height;
                            });
                            _lastRect = rect;
                        }

                        // If game is foreground window and overlay is hidden, show it
                        if (hWndFg == ProcessManager.GameProcess.MainWindowHandle && !topMost.Result)
                        {
                            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                            {
                                Topmost = true;
                                IsVisible = true;
                                Focus();
                                SetLayeredWindowAttributes(this.TryGetPlatformHandle().Handle, 0, 255, 0x2);
                            });
                        }
                        // Otherwise hide the overlay
                        else if (hWndFg != ProcessManager.GameProcess.MainWindowHandle && topMost.Result)
                        {
                            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                            {
                                Topmost = false;
                                IsVisible = false;
                                SetLayeredWindowAttributes(this.TryGetPlatformHandle().Handle, 0, 0, 0x2);
                            });
                        }
                    }
                    else
                    {
                        // Game not running or not focused, disable overlay
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            Topmost = false;
                            IsVisible = false;
                        });
                    }
                }
                catch (ThreadAbortException)
                {

                }
                catch (System.Exception ex)
                {
                    Log.WriteLine($"Exception occured during overlay update: {ex.Message}");
                }
                finally
                {
                    Thread.Sleep(100);
                }
        }
    }
}
