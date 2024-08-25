using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using WoWmapperX.Overlay;
using WoWmapperX.AvaloniaImpl;

namespace WoWmapperX.SettingsPanels
{
    public partial class SettingsOverlay : UserControl
    {
        public SettingsOverlay()
        {
            InitializeComponent();

            if(App.IsWine())
            { 
                FeatureText.Text = "Wine environment detected:\nThis feature does not work on wine due to unexpected behaviors with some\nwindows functions. Click here for more details.";
                FeatureText.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                FeatureText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.CornflowerBlue);
                FeatureText.TextDecorations = Avalonia.Media.TextDecorations.Underline;
                FeatureText.Tapped += FeatureText_Tapped;
                CheckEnableOverlay.IsEnabled = false;
                AppSettings.Default.EnableOverlay = false;
            }
        }

        private void FeatureText_Tapped(object sender, RoutedEventArgs e)
        { 
            Process.Start(new ProcessStartInfo("https://github.com/leoaviana/WoWmapper/wiki/Advanced-Features") { UseShellExecute = true });
        }

        private void CheckEnableOverlay_OnChecked(object sender, RoutedEventArgs e)
        {
            App.Overlay.Start();
        }

        private void CheckEnableOverlay_OnUnchecked(object sender, RoutedEventArgs e)
        {
            App.Overlay.Stop();
        }

        private void Alignment_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ComboHorizontal == null || ComboVertical == null) return;
            App.Overlay.SetAlignment((HorizontalAlignment)ComboHorizontal.SelectedIndex + 1, (VerticalAlignment)ComboVertical.SelectedIndex + 1);
        }

        private void TestNotification_Click(object sender, RoutedEventArgs e)
        {
            App.Overlay.PopupNotification(new OverlayNotification()
            {
                Header = "This is a test",
                Content = "This is a test notification. You clicked a button, and this is the notification that appeared. That's it.",
                Duration = 5000
            });
        }
    }
}
