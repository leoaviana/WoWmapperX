using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Material.Styles.Themes; 
using System.Collections.Generic;
using Material.Styles.Themes.Base;
using System;
using WoWmapperX.Views;
using System.Linq;
using System.Reflection;
using Avalonia.Interactivity;
using WoWmapperX.ConsolePort;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using WoWmapperX.AvaloniaImpl;

namespace WoWmapperX.SettingsPanels
{
    public partial class SettingsWoWmapper : UserControl
    {        
        private bool _ready;
        public SettingsWoWmapper()
        {
            InitializeComponent();

            List<String> l = new List<string>();
            l.Add("Dark");
            l.Add("Light");

            var currentTheme = AppSettings.Default.AppTheme;
            var currentAccent = AppSettings.Default.AppAccent;

            var comboAccentItems = Enum.GetNames(typeof(Material.Colors.PrimaryColor)).ToList();
            ComboTheme.ItemsSource = l; 
            ComboAccent.ItemsSource = comboAccentItems;

            ComboTheme.SelectedIndex = currentTheme == "Dark" ? 0 : 1;
            ComboAccent.SelectedIndex = comboAccentItems.IndexOf(currentAccent);

            TextVersion.Text = $"WoWmapper Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

            _ready = true;
        } 

        private void ComboTheme_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Design.IsDesignMode) return;

            var stheme = ComboTheme.SelectedItem as string;

            var theme = new Theme(); 
            var themeBase = Application.Current.LocateMaterialTheme<MaterialThemeBase>();

            if (ComboAccent.SelectedItem != null)
            {
                Avalonia.Media.Color color(string a) => Material.Colors.SwatchHelper.Lookup[(Material.Colors.MaterialColor)Enum.Parse(typeof(Material.Colors.PrimaryColor), a)];
                theme.SetPrimaryColor(color(ComboAccent.SelectedItem.ToString()));
                theme.SetSecondaryColor(color(ComboAccent.SelectedItem.ToString()));

                AppSettings.Default.AppAccent = ComboAccent.SelectedItem.ToString();

                if (MainWindow.Instance.TitleBar != null)
                {
                    if (ComboAccent.SelectedItem.ToString() == "Lime" || ComboAccent.SelectedItem.ToString() == "Amber" || ComboAccent.SelectedItem.ToString() == "Yellow")
                        MainWindow.Instance.TitleBar.MainBackground.Background = new Avalonia.Media.SolidColorBrush(color("Orange"));
                    else
                        MainWindow.Instance.TitleBar.MainBackground.Background = new Avalonia.Media.SolidColorBrush(color(ComboAccent.SelectedItem.ToString()));
                }
            }


            if (stheme == "Dark") 
                theme.SetBaseTheme(BaseThemeMode.Dark.GetBaseTheme()); 
            else 
                theme.SetBaseTheme(BaseThemeMode.Light.GetBaseTheme());

            AppSettings.Default.AppTheme = theme.GetBaseThemeMode() == BaseThemeMode.Dark ? "Dark" : "Light";

            AppSettings.Default.Save();

            themeBase.CurrentTheme = theme;

            App.InvokeThemeChanged();
        }

        private async void ButtonResetAll_Click(object sender, RoutedEventArgs e)
        {
            /*
            var warn = System.Windows.Forms.MessageBox.Show("Your settings will be reset and WoWmapper will exit.\n\nContinue?", "Warning",
                System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
            if (warn == System.Windows.Forms.DialogResult.No) return;
            */

            var warn = await MessageBox.Show("Your settings will be reset and WoWmapper will exit.\n\nContinue?", "Warning", MessageBoxButton.YesNo);
            if(warn == MessageBoxResult.No) return;

            AppSettings.Default.Reset();
            AppSettings.Default.Save();
            ((IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current.ApplicationLifetime).Shutdown(0);
        }

        private void CheckExportBindings_OnChecked(object sender, RoutedEventArgs e)
        {
            BindWriter.WriteBinds();
        }
        
        private async void CheckOverrideXinput_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!_ready) return;
            /* System.Windows.Forms.MessageBox.Show("Please restart WoWmapper for changes to this setting to take effect.", "Notice",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information); */
            await MessageBox.Show("Please restart WoWmapper for changes to this setting to take effect.", "Notice", MessageBoxButton.OK);
        }
    }
}
