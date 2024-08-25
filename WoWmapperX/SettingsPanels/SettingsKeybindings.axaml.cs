using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Styling; 
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using WoWmapperX.AvaloniaImpl;
using WoWmapperX.ConsolePort;
using WoWmapperX.Controllers;
using WoWmapperX.Keybindings; 
using WoWmapperX.Views;
using SettingsKeybindingsViewModel = WoWmapperX.ViewModels.SettingsKeybindingsViewModel;

namespace WoWmapperX.SettingsPanels
{
    public partial class SettingsKeybindings : UserControl
    {
        private readonly SettingsKeybindingsViewModel.ViewModel _viewModel;
        private Animation fadeOut = new Animation { Children = {  new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = 0d } }, Cue = new Cue(1d)}}};
        private Animation fadeIn = new Animation { Children = { new KeyFrame() { Setters = { new Setter { Property = Visual.OpacityProperty, Value = 1d } },Cue = new Cue(1d)}}};

        public SettingsKeybindings()
        {
            DataContext = _viewModel = new SettingsKeybindingsViewModel.ViewModel();
            InitializeComponent();

            ListKeybinds.PointerPressed += ListKeybinds_Click;
            ListKeybinds.PointerMoved += ListKeybinds_PointerOver;
            ListKeybinds.PointerExited += ListKeybinds_PointerLeave;

            fadeIn.Duration = fadeOut.Duration = new System.TimeSpan(0, 0, 0, 0, 200); 

            MainWindow.ButtonStyleChanged += RefreshBinds; 

            BindManager.BindingsChanged += (sender, args) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => RefreshBinds()); 
            };

            DoTransition();


            TextSyncMessage.Text = AppSettings.Default.ExportBindings
                        ? "ConsolePort sync is enabled. Your settings will be automatically synced when changed. Restart WoW or type /reload in game to load changes."
                        : "ConsolePort sync is disabled. You will need to manually configure the controller calibration within World of Warcraft before you can play.";

            AppSettings.Default.SettingChanging += (sender, args) =>
            {
                if (args.SettingName == "ExportBindings")
                {
                    TextSyncMessage.Text = (bool)args.NewValue
                        ? "ConsolePort sync is enabled. Your settings will be automatically synced when changed. Restart WoW or type /reload in game to load changes."
                        : "ConsolePort sync is disabled. You will need to manually configure the controller calibration within World of Warcraft before you can play.";
                }
            };
        }

        #region "'ListView' stuff"
        private void ListKeybinds_Click(object sender, PointerPressedEventArgs e)
        {
            object item = (e.Source as Control)?.DataContext;
            _viewModel.SelectedItem = (SettingsKeybindingsViewModel.Item)item;
        }


        object oldItem = null;
        private void ListKeybinds_PointerOver(object sender, PointerEventArgs e)
        {
            object item = (e.Source as Control)?.DataContext;
            if (oldItem != null)
            {
                if (_viewModel.SelectedItem == null)
                {
                    ((SettingsKeybindingsViewModel.Item)oldItem).SelectedOpacity = 0;
                }
                else if (_viewModel.SelectedItem.Index != ((SettingsKeybindingsViewModel.Item)oldItem).Index)
                    ((SettingsKeybindingsViewModel.Item)oldItem).SelectedOpacity = 0;
                oldItem = null;
            }
            try
            {
                if (_viewModel.SelectedItem == null)
                {
                    ((SettingsKeybindingsViewModel.Item)item).SelectedOpacity = 0.2;
                    oldItem = item;
                }
                else if (_viewModel.SelectedItem.Index != ((SettingsKeybindingsViewModel.Item)item).Index)
                {
                    ((SettingsKeybindingsViewModel.Item)item).SelectedOpacity = 0.2;
                    oldItem = item;
                }
            }
            catch { }
        }

        private void ListKeybinds_PointerLeave(object sender, PointerEventArgs e)
        {
            if (oldItem != null)
            {
                if (_viewModel.SelectedItem == null)
                {
                    ((SettingsKeybindingsViewModel.Item)oldItem).SelectedOpacity = 0;
                }
                else if (_viewModel.SelectedItem.Index != ((SettingsKeybindingsViewModel.Item)oldItem).Index)
                    ((SettingsKeybindingsViewModel.Item)oldItem).SelectedOpacity = 0;
                oldItem = null;
            }
        }

        #endregion

        private void CheckCustomBinding_Changed(object sender, RoutedEventArgs e)
        {
            if (ListKeybinds == null) return;
            CheckCustomBinding();
        } 
        private async void CheckCustomBinding()
        {
            // Workaround for IsChecked binding seeming to be set only after Checked event in avalonia.

            await System.Threading.Tasks.Task.Delay(5);  

            BindManager.ResetDefaults(AppSettings.Default.ModifierStyle);
            AppSettings.Default.BindingsModified = System.DateTime.Now;
            BindWriter.WriteBinds();
            RefreshBinds();

            DoTransition();
        }
        private void DoTransition()
        { 
            if (AppSettings.Default.CustomBindings)
            {
                /*
                // Show custom bindings panel 
                fadeIn.Apply(ListKeybinds, null, Observable.Return(true), () =>
                {
                    ListKeybinds.IsEnabled = true;
                    ListKeybinds.Opacity = 1;
                });
                fadeIn.Apply(ButtonResetBinds, null, Observable.Return(true), () =>
                {
                    ButtonResetBinds.Opacity = 1;
                    ButtonResetBinds.IsEnabled = true;
                });
                */
                    
                fadeIn.RunAsync(ListKeybinds).ContinueWith(t =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        ListKeybinds.IsEnabled = true;
                        ListKeybinds.Opacity = 1;
                    });
                });
                fadeIn.RunAsync(ButtonResetBinds).ContinueWith(t =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        ButtonResetBinds.Opacity = 1;
                        ButtonResetBinds.IsEnabled = true;
                    });
                });
            }
            else if (!AppSettings.Default.CustomBindings)
            {
                /*
                // Hide custom bindings panel
                fadeOut.Apply(ListKeybinds, null, Observable.Return(true), () =>
                {
                    ListKeybinds.IsEnabled = false;
                    ListKeybinds.Opacity = 0;
                });
                fadeOut.Apply(ButtonResetBinds, null, Observable.Return(true), () =>
                {
                    ButtonResetBinds.IsEnabled = false;
                    ButtonResetBinds.Opacity = 0;
                });
                */

                fadeOut.RunAsync(ListKeybinds).ContinueWith(t =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        ListKeybinds.IsEnabled = false;
                        ListKeybinds.Opacity = 0;
                    }
                    );
                });
                fadeOut.RunAsync(ButtonResetBinds).ContinueWith(t =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        ButtonResetBinds.IsEnabled = false;
                        ButtonResetBinds.Opacity = 0;
                    });
                });
            } 

            RefreshBinds();
        }

        List<Keybind> _binds = new List<Keybind>();

        private void RefreshBinds()
        {
            _binds = BindManager.CurrentKeybinds.ToList();
            _viewModel.Empty();

            var shoulders = new List<GamepadButton>
            {
                GamepadButton.ShoulderLeft,
                GamepadButton.ShoulderRight,
                GamepadButton.TriggerLeft,
                GamepadButton.TriggerRight,
            };

            switch (AppSettings.Default.ModifierStyle)
            {
                case 0:
                    shoulders.Remove(GamepadButton.ShoulderRight);
                    shoulders.Remove(GamepadButton.TriggerRight);
                    break;
                case 1:
                    shoulders.Remove(GamepadButton.ShoulderLeft);
                    shoulders.Remove(GamepadButton.ShoulderRight);
                    break;
                case 2:
                    shoulders.Remove(GamepadButton.ShoulderLeft);
                    shoulders.Remove(GamepadButton.TriggerLeft);
                    break;
                case 3:
                    shoulders.Remove(GamepadButton.TriggerLeft);
                    shoulders.Remove(GamepadButton.TriggerRight);
                    break;
            }

            _binds.RemoveAll(bind => shoulders.Contains(bind.BindType));

            var keybindList = new List<SettingsKeybindingsViewModel.KeybindListItem>();

            foreach (var bind in _binds)
            {
                var buttonBind = bind.Key.ToString();
                var buttonText = ControllerManager.GetButtonName(bind.BindType);
                var buttonIcon = ControllerManager.GetButtonIcon(bind.BindType);


                var buttonItem = new SettingsKeybindingsViewModel.KeybindListItem();
                buttonItem.Key = buttonBind;
                buttonItem.BindType = buttonText;
                buttonItem.Image = buttonIcon;

                keybindList.Add(buttonItem);
            }
             
            _viewModel.AddRange(keybindList); 
        }

        private void ButtonResetBinds_OnClick(object sender, RoutedEventArgs e)
        {
            BindManager.ResetDefaults(AppSettings.Default.ModifierStyle);
            AppSettings.Default.BindingsModified = System.DateTime.Now;
            BindWriter.WriteBinds();
            RefreshBinds();
        }

        private void ComboButtonIcons_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboButtonIcons == null) return;

            DoTransition();
            AppSettings.Default.BindingsModified = System.DateTime.Now;
            BindWriter.WriteBinds();
            MainWindow.UpdateButtonStyle();
        }

        private void ComboCurrentStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboCurrentStyle == null) return;

            AppSettings.Default.BindingsModified = System.DateTime.Now;
            BindManager.ResetDefaults(ComboCurrentStyle.SelectedIndex);
            BindWriter.WriteBinds();
            DoTransition();
        }

        public void ListKeybinds_OnMouseDoubleClick(object sender, TappedEventArgs e)
        { 
            if (_viewModel.SelectedItem == null) return;

            MainWindow.ShowKeybindDialog(_binds[_viewModel.SelectedItem.Index].BindType);
        }
    }
}
