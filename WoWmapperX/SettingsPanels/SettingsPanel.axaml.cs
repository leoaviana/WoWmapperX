using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Material.Styles.Themes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Linq;
using WoWmapperX.AvaloniaImpl;

namespace WoWmapperX.SettingsPanels
{
    public partial class SettingsPanel : UserControl
    { 
        public SettingsPanel()
        {
            InitializeComponent(); 
            
            App.CurrentThemeChanged += (object sender, EventArgs e) => {
                MenuStyleChanged();
            };

            #region Control event binding
            DrawerList.PointerReleased += DrawerSelectionChanged;
            DrawerList.KeyUp += DrawerList_KeyUp;
            #endregion
        }

        private void DrawerList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
                DrawerSelectionChanged(sender, null);
        }

        public void DrawerSelectionChanged(object sender, RoutedEventArgs args)
        {
            if (sender == null) return;

            var listBox = sender as ListBox;
            if (!listBox.IsFocused && !listBox.IsKeyboardFocusWithin)
                return;
            try
            {
                PageCarousel.SelectedIndex = listBox.SelectedIndex;
                mainScroller.Offset = Vector.Zero;
                mainScroller.VerticalScrollBarVisibility =
                    listBox.SelectedIndex == 5 ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;

            }
            catch
            {
            }
            NavDrawerSwitch.IsChecked = false;
        }

        private void MenuStyleChanged ()
        {
            var themeBase = Application.Current.LocateMaterialTheme<MaterialThemeBase>();
            if (themeBase == null) return;
             
            MenuIcon.Fill = new Avalonia.Media.SolidColorBrush(themeBase.CurrentTheme.PrimaryMid.ForegroundColor); 

        }
    }
}
