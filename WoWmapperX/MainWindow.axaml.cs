using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Material.Styles.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWmapperX.AvaloniaImpl;
using WoWmapperX.AvaloniaViews;
using WoWmapperX.Controllers;
using WoWmapperX.Keybindings;

namespace WoWmapperX.Views
{
    public partial class MainWindow : Window
    {

        public delegate void ButtonStyleChangedHandler();
        public delegate void ShowKeybindDialogHandler(GamepadButton button);

        private readonly List<Key> _ignoreKeys = new()
        {
            Key.Escape,
            Key.LeftAlt,
            Key.RightAlt,
            Key.LeftCtrl,
            Key.RightCtrl,
            Key.LeftShift,
            Key.RightShift,
            Key.LWin,
            Key.RWin
        }; 
         
        private GamepadButton _keybindButton;

        private Key _keybindKey = Key.None;

        public static MainWindow Instance { get; set; }

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            TitleBar.SettingsButtonPressed = SettingsButtonPressed;
            ShowKeybindDialogEvent += OnShowKeybindDialog;
            Activated += MainWindow_Activated;
            Deactivated += MainWindow_Deactivated;

            LoadTheme();
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            try
            {
                static Avalonia.Media.Color color(string a) => Material.Colors.SwatchHelper.Lookup[(Material.Colors.MaterialColor)Enum.Parse(typeof(Material.Colors.PrimaryColor), a)];

                if (AppSettings.Default.AppAccent == "Lime" || AppSettings.Default.AppAccent == "Amber" || AppSettings.Default.AppAccent == "Yellow")
                    MainWindow.Instance.TitleBar.MainBackground.Background = new Avalonia.Media.SolidColorBrush(color("Orange"));
                else
                    MainWindow.Instance.TitleBar.MainBackground.Background = new Avalonia.Media.SolidColorBrush(color(AppSettings.Default.AppAccent));
            }
            catch { }


        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            try
            {
                MainWindow.Instance.TitleBar.MainBackground.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Gray);
            }
            catch { }
        }

        private void LoadTheme()
        {
            var themeBase = Application.Current.LocateMaterialTheme<MaterialThemeBase>();

            static Avalonia.Media.Color color(string a) => Material.Colors.SwatchHelper.Lookup[(Material.Colors.MaterialColor)Enum.Parse(typeof(Material.Colors.PrimaryColor), a)];
            var primaryColor = (color(AppSettings.Default.AppAccent));
            var secondaryColor = (color(AppSettings.Default.AppAccent));

            if (AppSettings.Default.AppTheme == "Dark")
                themeBase.CurrentTheme = Material.Styles.Themes.Theme.Create(Material.Styles.Themes.Theme.Dark, primaryColor, secondaryColor);
            else
                themeBase.CurrentTheme = Material.Styles.Themes.Theme.Create(Material.Styles.Themes.Theme.Light, primaryColor, secondaryColor);

            try
            {

                if (AppSettings.Default.AppAccent == "Lime" || AppSettings.Default.AppAccent == "Amber" || AppSettings.Default.AppAccent == "Yellow")
                    MainWindow.Instance.TitleBar.MainBackground.Background = new Avalonia.Media.SolidColorBrush(color("Orange"));
                else
                    MainWindow.Instance.TitleBar.MainBackground.Background = new Avalonia.Media.SolidColorBrush(color(AppSettings.Default.AppAccent));
            }
            catch { }

            App.InvokeThemeChanged();
        }

        private int SettingsButtonPressed()
        {
            PageCarousel.SelectedIndex = PageCarousel.SelectedIndex == 1 ? 0 : 1;

            return 0;
        }

        public static event ButtonStyleChangedHandler ButtonStyleChanged;
        public static event ShowKeybindDialogHandler ShowKeybindDialogEvent;

        public static void UpdateButtonStyle()
        {
            if (ButtonStyleChanged != null)
                Avalonia.Threading.Dispatcher.UIThread.Post(() => ButtonStyleChanged());
        } 


        public static void ShowKeybindDialog(GamepadButton button)
        {
            ShowKeybindDialogEvent?.Invoke(button);
        }

        private void OnShowKeybindDialog(GamepadButton button)
        {
            _keybindButton = button;

            var buttonName = ControllerManager.GetButtonName(button);

            KeyDown += OnKeyDown; 

            _keybindDialogController.PrepareDialog($"Rebind {buttonName}",
                $"Press a button on your keyboard. Some special keys may not be recognized by the WoW client.",
                KeybindDialogControllerOnCanceled,
                KeybindDialogControllerOnClosed
                );

            _keybindDialogController.ShowDialog();
             
        }

        private void KeybindDialogControllerOnCanceled()
        {
            _keybindKey = Key.None;
            _keybindDialogController.CloseDialog();
        }

        private void KeybindDialogControllerOnClosed()
        {
            if (_keybindKey != Key.None)
            {
                //User pressed a key, save keybinding
                BindManager.SetKey(_keybindButton, _keybindKey);
                Console.WriteLine(_keybindKey.ToString());
            } 
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (_keybindDialogController == null) return;

            // Handle keyboard input for keybinding dialog
            if (keyEventArgs.Key == Key.Escape)
            {
                _keybindKey = Key.None;
            }
            else
            {
                if (keyEventArgs.KeyModifiers == KeyModifiers.None) 
                    _keybindKey = keyEventArgs.Key;
            }

            if (_ignoreKeys.Contains(_keybindKey)) return;

            if (_keybindDialogController.IsVisible)
                _keybindDialogController.CloseDialog();
             
        }



        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (AppSettings.Default.RunInBackground)
            {
                e.Cancel = true; 
                Hide();
            }
            (PageCarousel.Items.Cast<object>().ToList()[0] as MainPage)._uiTimer.Stop();
            base.OnClosing(e);
        }

    }
}
