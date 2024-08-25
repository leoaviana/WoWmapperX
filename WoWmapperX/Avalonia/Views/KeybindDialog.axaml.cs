using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Material.Styles.Themes;
using System.Reactive.Linq;

namespace WoWmapperX.AvaloniaViews
{
    public partial class KeybindDialog : UserControl
    {
        public enum ActionType
        {
            Ok,
            Cancel
        }
        private System.Action ActionClicked;
        private System.Action DialogClosed;

        private Animation fadeOut = new Animation { FillMode = FillMode.Forward, Children = { new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = 1d } }, Cue = new Cue(0d) }, new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = 0d } }, Cue = new Cue(1d) } } };
        private Animation fadeIn = new Animation { FillMode = FillMode.Forward, Children = { new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = 0d } }, Cue = new Cue(0d) }, new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = 1d } }, Cue = new Cue(1d) } } };

        private Animation fadeOutBgLayer = new Animation { FillMode = FillMode.Forward, Children = { new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = .5d } }, Cue = new Cue(0d) }, new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = 0d } }, Cue = new Cue(1d) } } };
        private Animation fadeInBgLayer = new Animation { FillMode = FillMode.Forward, Children = { new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = 0d } }, Cue = new Cue(0d) }, new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = .5d } }, Cue = new Cue(1d) } } };

        public KeybindDialog()
        {
            InitializeComponent();
            fadeOut.Duration = new System.TimeSpan(0, 0, 0, 0, 200);
            fadeIn.Duration = fadeOut.Duration;
            fadeInBgLayer.Duration = fadeOut.Duration;
            fadeOutBgLayer.Duration = fadeOut.Duration;
        }

        public void PrepareDialog(string contentHeader, string contentText, System.Action actionClicked = null, System.Action dialogClosed = null, ActionType actionType = ActionType.Cancel)
        {
            ContentHeader.Text = contentHeader;
            ContentText.Text = contentText;
            switch(actionType)
            {
                case ActionType.Ok:
                    ActionButton.Content = "Ok";
                    break;
                case ActionType.Cancel:
                    ActionButton.Content = "Cancel";
                    break; 
            }

            ActionClicked = actionClicked;
            DialogClosed = dialogClosed;
        }

        public async void ShowDialog()
        {
            
            var theme = Application.Current.LocateMaterialTheme<MaterialThemeBase>();

            DialogLayer.Background = new Avalonia.Media.SolidColorBrush(theme.CurrentTheme.Background);
            bgLayer.Background = new Avalonia.Media.SolidColorBrush(theme.CurrentTheme.Body);


            bgLayer.Opacity = 0;
            DialogLayer.Opacity = 0;

            this.IsVisible = true;
            /*
            fadeInBgLayer.Apply(bgLayer, null, Observable.Return(true), () =>
            {
                bgLayer.Opacity = 0.5;
                fadeIn.Apply(DialogLayer, null, Observable.Return(true), () =>
                {
                    DialogLayer.Opacity = 1d; 
                });
            }); 

            fadeInBgLayer.RunAsync(bgLayer).ContinueWith(t =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    bgLayer.Opacity = 0.5;

                    fadeIn.RunAsync(DialogLayer).ContinueWith(t =>
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            DialogLayer.Opacity = 1d;
                        });
                    });
                });
            });
            */

            await fadeInBgLayer.RunAsync(bgLayer); 
            await fadeIn.RunAsync(DialogLayer); 
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionClicked != null)
                ActionClicked();
            else
                CloseDialog();
        }

        public async void CloseDialog()
        {
            /*
            fadeOut.Apply(DialogLayer, null, Observable.Return(true), () =>
            {
                DialogLayer.Opacity = 0d;
                fadeOut.Apply(bgLayer, null, Observable.Return(true), () =>
                {
                    bgLayer.Opacity = 0d;
                    if(DialogClosed != null)
                        DialogClosed();
                    this.IsVisible = false;
                });
            });
            
            fadeOut.RunAsync(DialogLayer).ContinueWith(t =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    DialogLayer.Opacity = 0d;

                    fadeOut.RunAsync(bgLayer).ContinueWith(t =>
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            bgLayer.Opacity = 0d;
                            if (DialogClosed != null)
                                DialogClosed();
                            this.IsVisible = false;
                        });
                    });
                });
            });
            */

            await fadeOut.RunAsync(DialogLayer);  
            await fadeOutBgLayer.RunAsync(bgLayer); 
            if (DialogClosed != null)
                DialogClosed();
            this.IsVisible = false;
        }
    }
}
