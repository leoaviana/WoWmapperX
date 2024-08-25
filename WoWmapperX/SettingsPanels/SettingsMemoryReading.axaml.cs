using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Material.Styles.Themes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using WoWmapperX.Controllers;
using WoWmapperX.Keybindings;
using WoWmapperX.AvaloniaImpl;
using WoWmapperX.Views;
using WoWmapperX.WorldOfWarcraft;
using WoWmapperX.WoWInfoReader;

namespace WoWmapperX.SettingsPanels
{
 
    public partial class SettingsMemoryReading : UserControl
    {
        private bool _ready; 
        private List<GamepadButton> _buttons = new List<GamepadButton>();

        public SettingsMemoryReading()
        {
            InitializeComponent();

            StackWarning.IsVisible = !AppSettings.Default.EnableMemoryReading;

            MainWindow.ButtonStyleChanged += ButtonStyleChanged;

            
            var theme = Application.Current.LocateMaterialTheme<MaterialThemeBase>();

            App.CurrentThemeChanged += (object sender, EventArgs e) => {
                ButtonStyleChanged();
            };

            ButtonStyleChanged();
            _ready = true;
        }

        private void ButtonStyleChanged()
        {
            _ready = false;
            ImageMenuConfirm.Source = ControllerManager.GetButtonIcon(GamepadButton.RFaceDown);
            ImageMenuUp.Source = ControllerManager.GetButtonIcon(GamepadButton.LFaceUp);
            ImageMenuDown.Source = ControllerManager.GetButtonIcon(GamepadButton.LFaceDown);


            var _binds = BindManager.CurrentKeybinds.ToList();
            ComboAoeCancel.ItemsSource = null;
            ComboAoeConfirm.ItemsSource = null;
            _buttons.Clear();

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
            _binds.RemoveAll(bind => bind.BindType.ToString().StartsWith("LeftStick"));
            var comboItemsConfirm = new List<ComboBoxItem>();
            var comboConfirmSelected = 0;
            var comboItemsCancel = new List<ComboBoxItem>();
            var comboCancelSelected = 0;

            foreach (var bind in _binds)
            {
                foreach (ComboBox box in new[] { ComboAoeConfirm, ComboAoeCancel })
                {
                    var buttonItem = new ComboBoxItem
                    {
                        Content = new DockPanel
                        {
                            Children =
                            {
                                new Image
                                {
                                    Source = ControllerManager.GetButtonIcon(bind.BindType),
                                    Width = 24,
                                    Height = 24,
                                    Stretch = Avalonia.Media.Stretch.Uniform
                                },
                                new TextBlock
                                {
                                    Text = ControllerManager.GetButtonName(bind.BindType),
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                    Margin=new Thickness(5,0,0,0)
                                }
                            }
                        }
                    };
                    if (AppSettings.Default.MemoryAoeConfirm == bind.BindType && box.Name == "ComboAoeConfirm") buttonItem.IsSelected = true;
                    if (AppSettings.Default.MemoryAoeCancel == bind.BindType && box.Name == "ComboAoeCancel") buttonItem.IsSelected = true;

                    if (box.Name == "ComboAoeConfirm")
                    {
                        comboItemsConfirm.Add(buttonItem);
                        if (buttonItem.IsSelected)
                            comboConfirmSelected = comboItemsConfirm.IndexOf(buttonItem);
                    }
                    else
                    {
                        comboItemsCancel.Add(buttonItem);
                        if (buttonItem.IsSelected)
                            comboCancelSelected = comboItemsCancel.IndexOf(buttonItem);
                    }

                }


                _buttons.Add(bind.BindType);
            }
            ComboAoeConfirm.ItemsSource = comboItemsConfirm;
            ComboAoeConfirm.SelectedIndex = comboConfirmSelected; 
            ComboAoeCancel.ItemsSource = comboItemsCancel;
            ComboAoeCancel.SelectedIndex = comboCancelSelected;

            _ready = true;
        }
        public void TextMoreInfo_Click(object sender, TappedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/leoaviana/WoWmapper/wiki/Advanced-Features") { UseShellExecute = true }); 
        }


        private void HideWarning(object sender, RoutedEventArgs e)
        {
            if (StackWarning == null) return;
            StackWarning.IsVisible = false; 
        }

        private void ShowWarning(object sender, RoutedEventArgs e)
        {
            if (StackWarning == null) return;
            StackWarning.IsVisible = true;
        }

        private async void ButtonRefreshValues_Click(object sender, RoutedEventArgs e)
        {
            ListDebug.ItemsSource = null;
            var ListDebugItems = new List<DebugItem>();

            try
            {
                var health = WoWReader.PlayerHealth;
                ListDebugItems.Add(new DebugItem()
                {
                    Name = "PlayerBase",
                    Address = WoWReader.offsetPlayerBase.ToString("X2"),
                    Value = $"{health.Item1}/{health.Item2}"
                });
                ListDebugItems.Add(new DebugItem()
                {
                    Name = "GameState",
                    Address = WoWReader.offsetGameState.ToString("X2"),
                    Value = WoWReader.GameState.ToString()
                });
                ListDebugItems.Add(new DebugItem()
                {
                    Name = "LoggedState",
                    Address = WoWReader.offsetLoggedState.ToString("X2"),
                    Value = WoWReader.LoggedState.ToString()
                });
                ListDebugItems.Add(new DebugItem()
                {
                    Name = "MouselookState",
                    Address = WoWReader.offsetMouselookState.ToString("X2"),
                    Value = WoWReader.MouselookState.ToString()
                });
                ListDebugItems.Add(new DebugItem()
                {
                    Name = "WalkState",
                    Address = WoWReader.offsetWalkState.ToString("X2"),
                    Value = WoWReader.MovementState.ToString()
                });
                ListDebugItems.Add(new DebugItem()
                {
                    Name = "AoeState",
                    Address = WoWReader.offsetAoeState.ToString("X2"),
                    Value = WoWReader.AoeState.ToString()
                });
            }
            catch (System.Exception ex)
            {
                if (ProcessManager.GameRunning)
                {
                    /*
                    System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error", System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);
                    */
                    await MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK);
                }
                else
                {
                    /*
                    var result = System.Windows.Forms.MessageBox.Show("An error happened, seems like the game is not running, do you want to see detailed exception information?", "Error", System.Windows.Forms.MessageBoxButtons.YesNo,
                        System.Windows.Forms.MessageBoxIcon.Error);

                    if (result == System.Windows.Forms.DialogResult.Yes)
                        System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error", System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Error);
                    */

                    var result = await MessageBox.Show("An error happened, seems like the game is not running, do you want to see detailed exception information?", "Error", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                        await MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK);
                }

            }

            ListDebug.ItemsSource = ListDebugItems;
        }

        private void AoeOverride_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_ready) return;
            if(ComboAoeCancel.SelectedIndex != -1)
                AppSettings.Default.MemoryAoeCancel = _buttons[ComboAoeCancel.SelectedIndex];

            if(ComboAoeConfirm.SelectedIndex != -1)
                AppSettings.Default.MemoryAoeConfirm = _buttons[ComboAoeConfirm.SelectedIndex];
        }


    }

    public class DebugItem
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Value { get; set; }
    }
}
