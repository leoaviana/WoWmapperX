using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using System.Reactive.Linq;
using System.Threading;

namespace WoWmapperX.Overlay
{
    public partial class OverlayToast : UserControl
    {
        private const double PopupOpacity = 1;
        private readonly int _duration; 
        private Animation fadeOut = new Animation { Children = { new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = 0d } }, Cue = new Cue(1d) } } };
        private Animation fadeIn = new Animation { Children = { new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = 1d } }, Cue = new Cue(1d) } } };

        public OverlayNotification BaseNotification { get; private set; }
        public event System.EventHandler NotificationCompleted;

        public OverlayToast()
        {
            if (Design.IsDesignMode)
            {
                InitializeComponent();
            } 
        }
        public OverlayToast(OverlayNotification notification)
        {
                        
            BaseNotification = notification;
            _duration = notification.Duration;
            fadeIn.Duration = new System.TimeSpan(0,0,0,0, notification.FadeIn);
            fadeOut.Duration = new System.TimeSpan(0,0,0,0, notification.FadeOut); 
            InitializeComponent();
            Opacity = 0;

            if (notification.Image != null)
            {
                ImageIcon.Source = notification.Image;
            }
            else
            {
                ImageIcon.Source = OverlayIcons.Random();
            }

            NotificationHeader.Text = notification.Header;
            NotificationText.Text = notification.Content; 

            //UpdateLayout()

            Avalonia.Threading.Dispatcher.UIThread.Post(FadeIn);
            new Thread(() =>
            {
                Thread.Sleep(_duration);
                try
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(FadeOut);
                }
                catch { }
            }).Start();
        }

        public void FadeIn()
        {
            ImageBackground.Height = NotificationText.Height;

            /*
            fadeIn.Apply(this, null, Observable.Return(true), () => {
                Opacity = PopupOpacity; 
            });
            */

            fadeIn.RunAsync(this).ContinueWith(t => 
            { 
                Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                { 
                    Opacity = PopupOpacity;
                }); 
            });
             
        }

        public void FadeOut()
        {
            /*
            fadeOut.Apply(this, null, Observable.Return(true), () => {
                Opacity = 0;
                IsVisible = false; 
                NotificationCompleted?.Invoke(this, null);  
            }); 
            */

            fadeOut.RunAsync(this).ContinueWith(t =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    Opacity = 0;
                    IsVisible = false;
                    NotificationCompleted?.Invoke(this, null);
                });
            });
        }

        public override void Render(DrawingContext drawingContext)
        {
            if(Design.IsDesignMode) { base.Render(drawingContext); return; }

            var finalSize = NotificationText.Bounds.Height + NotificationText.Margin.Top + 10;
            ImageBackground.Height = finalSize;
            var marg = ImageBackgroundBottom.Margin; 
            marg = new Thickness(marg.Left, finalSize, marg.Right, marg.Bottom);
            ImageBackgroundBottom.Margin = marg;
            base.Render(drawingContext);
        }
    }
}
