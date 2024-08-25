using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using WoWmapperX.Controllers;
using WoWmapperX.Views;
using WoWmapperX.WorldOfWarcraft;
using WoWmapperX.WoWInfoReader;
using WoWmapperX.AvaloniaImpl;
using System.Diagnostics;
using WoWmapperX.Controllers.DS4;

namespace WoWmapperX.AvaloniaViews
{
    public partial class MainPage : UserControl
    {
        #region "Main Window Stuff"   
        public readonly Timer _uiTimer = new Timer { AutoReset = true, Interval = 1000 }; 


        #endregion

        public MainPage()
        {
            InitializeComponent();
            InitializeMainWindow();
        }


        private void InitializeMainWindow()
        {
            TextVersion.Text = $"WoWmapper Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

            // Hide donation panel if it's been deactivated
            if (AppSettings.Default.DisableDonationButton) DonateButton.IsVisible = false;

            // Begin interface update timer
            _uiTimer.Elapsed += UiTimer_Elapsed;
            _uiTimer.Start();

        }


        private void UiTimer_Elapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // Update UI elements
            try
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(UpdateUi);
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Exception when updating interface:\n        {ex.Message}");
            }
        }

        private void UpdateUi()
        {
            PanelDonate.IsVisible = AppSettings.Default.DisableDonationButton
                ? false
                : true;

            var activeDevice = ControllerManager.ActiveController;

            if (activeDevice != null) // Controller connected
            {
                var battery = activeDevice.BatteryLevel > 100 ? 100 : activeDevice.BatteryLevel;

                if (activeDevice.Type == GamepadType.PlayStation)
                    TextControllerStatus1.Text = $"{(activeDevice as DS4Controller).GetDeviceName()} connected";
                else
                    TextControllerStatus1.Text = $"Xinput controller connected";

                TextControllerStatus2.Text = $"Battery is at {battery}%";

                if ((activeDevice.Type == GamepadType.Xbox) && ControllerManager.IsXInput9)
                {
                    TextControllerStatus3.Text =
                        "Your system is using the DirectX 9 Xinput library. You will not be able to use the Xbox Guide button.";
                    TextControllerStatus3.IsVisible = true;
                }
                else
                {
                    TextControllerStatus3.IsVisible = false;
                }
            }
            else
            {
                TextControllerStatus1.Text = "No active controller";
                TextControllerStatus2.Text = "No information available";
            } 

            TextWoWStatus1.Text = ProcessManager.GameRunning
                ? "World of Warcraft is running"
                : "World of Warcraft is not running";

            if (AppSettings.Default.EnableMemoryReading)
                TextWoWStatus2.Text = WoWReader.IsAttached
                    ? "Memory reading is enabled"
                    : "Memory reading is unavailable";
            else
                TextWoWStatus2.Text = "Memory reading is disabled";
        }

        public void TextUpdateLink1_Click(object sender, TappedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/leoaviana/WoWmapper/releases") { UseShellExecute = true });
        }
    }
}
