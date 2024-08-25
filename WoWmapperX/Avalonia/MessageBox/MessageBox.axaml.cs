using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using WoWmapperX.Views;
using Avalonia;

namespace WoWmapperX.AvaloniaImpl
{
    public partial class MessageBox : Window
    {
        public MessageBox(string message, string title, MessageBoxButton buttons)
        {
            InitializeComponent();
            MessageText.Text = message;
            Title = title;
            ConfigureButtons(buttons);
            ConfigureTitle();

            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); 

            var size = MessageText.DesiredSize;
            this.Width = size.Width + 40;  // Add some padding
            this.Height = size.Height + 100;  // Add some padding for buttons
        }

        private void ConfigureTitle()
        {
            this.TitleBar.WindowIcon.IsVisible = false;
            this.TitleBar.SystemChromeTitle.Margin = new Thickness(10, 0, 0, 0);
            this.TitleBar.SystemChromeTitle.Text = this.Title;
            this.TitleBar.SettingsButton.IsVisible = false;
            this.TitleBar.MinimizeButton.IsVisible = false; 
        }

        private void ConfigureButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    OkButton.IsVisible = true;
                    break;
                case MessageBoxButton.OKCancel:
                    OkButton.IsVisible = true;
                    CancelButton.IsVisible = true;
                    break;
                case MessageBoxButton.YesNo:
                    YesButton.IsVisible = true;
                    NoButton.IsVisible = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    YesButton.IsVisible = true;
                    NoButton.IsVisible = true;
                    CancelButton.IsVisible = true;
                    break;
            }
        }

        private void OnOkClick(object sender, RoutedEventArgs e) => Close(MessageBoxResult.OK);
        private void OnYesClick(object sender, RoutedEventArgs e) => Close(MessageBoxResult.Yes);
        private void OnNoClick(object sender, RoutedEventArgs e) => Close(MessageBoxResult.No);
        private void OnCancelClick(object sender, RoutedEventArgs e) => Close(MessageBoxResult.Cancel);

        public static async Task<MessageBoxResult> Show(string message, string title = "", MessageBoxButton buttons = MessageBoxButton.OK, Window owner = null)
        {
            var messageBox = new MessageBox(message, title, buttons);
            if (owner != null)
            {
                return await messageBox.ShowDialog<MessageBoxResult>(owner);
            }
            else
            {
                return await messageBox.ShowDialog<MessageBoxResult>(MainWindow.Instance);
            }
        }


    }

    public enum MessageBoxButton
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    public enum MessageBoxResult
    {
        None,
        OK,
        Cancel,
        Yes,
        No
    }
}
