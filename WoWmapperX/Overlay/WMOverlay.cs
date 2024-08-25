using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Layout;
using WoWmapperX.AvaloniaImpl;

namespace WoWmapperX.Overlay
{
    public class WMOverlay
    {
        private OverlayWindow _overlay;
        public bool IsRunning => _overlay != null;
        public bool CrosshairVisible  { 
            get 
            {
                var t = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => _overlay.GetImageCrosshair.IsVisible);
                t.Wait();
                return t.Result;
            } 
        }

        public WMOverlay()
        {
            
        }

        public void Start()
        {
            if (IsRunning) return;

            Log.WriteLine("Creating overlay window...");
            _overlay = new OverlayWindow(); 
            SetAlignment((HorizontalAlignment)AppSettings.Default.NotificationH + 1, (VerticalAlignment)AppSettings.Default.NotificationV + 1);
        }

        public void Stop()
        {
            if (!IsRunning) return;

            Log.WriteLine("Closing overlay window...");
            _overlay.CloseOverlay();
            _overlay = null;
        }

        public void PopupNotification(OverlayNotification notification)
        {
            if (!IsRunning) return;

            Log.WriteLine($"Show notification: {notification.Header}: {notification.Content}");
            _overlay.PopupNotification(notification);
        } 

        public void SetCrosshairState(bool visible, int x = 0, int y = 0)
        {
            if (!IsRunning) return;

            Log.WriteLine($"Set crosshair {visible}: {x}, {y}");
            _overlay.SetCrosshairState(visible, x, y);
        }

        public void SetAlignment(HorizontalAlignment h, VerticalAlignment v)
        { 

            if (!IsRunning) return;

            Log.WriteLine($"Aligning notifications: {h}, {v}");
            _overlay.GetStackNotifications.HorizontalAlignment = h;
            _overlay.GetStackNotifications.VerticalAlignment = v;
        }
    }
}
