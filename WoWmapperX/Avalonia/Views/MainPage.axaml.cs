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
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DynamicData; 

namespace WoWmapperX.AvaloniaViews
{
    public partial class MainPage : UserControl
    {
        #region "Main Window Stuff"   
        public readonly Timer _uiTimer = new Timer { AutoReset = true, Interval = 1000 };


        #endregion

        #region "Updater Stuff"
        public static string GetCurrentBuildType()
        {
#if BUILD_NATIVEAOT
        return "wowmapperx-x86-aot.zip";
#elif BUILD_NOAOT
        return "wowmapperx-x86-noaot.zip";
#else
            return "wowmapperx-x86-noaot-net.zip";
#endif
        }

        private static readonly HttpClient _httpClient = new HttpClient
        {
            DefaultRequestHeaders = { { "User-Agent", "WoWmapperX" } }
        };

        private UpdateRelease _latest = null;

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

            CheckForUpdates(); 

        }

        public async void CheckForUpdates()
        {
            string apiUrl = $"https://api.github.com/repos/leoaviana/WoWmapperX/releases/latest";

            try
            {
                // Fetch latest release info
                string jsonResponse = await _httpClient.GetStringAsync(apiUrl);
                using var document = JsonDocument.Parse(jsonResponse);
                var root = document.RootElement;

                string latestVersion = root.GetProperty("tag_name").GetString();
                string releaseName = root.GetProperty("name").GetString();
                string releaseNotes = root.GetProperty("body").GetString();

                Version version = new(Assembly.GetExecutingAssembly().GetName().Version.ToString(3)); 
                Version latest = new(latestVersion);

                if (latest > version)
                {
                    Log.WriteLine($"New version found. Current: {version.ToString(3)}, Latest: {latestVersion}");
                    
                    TextUpdateStatus1.Text = $"Version {latestVersion} is available now!";
                    TextUpdateStatus1.Cursor = new Avalonia.Input.Cursor(StandardCursorType.Hand);
                    TextUpdateStatus1.TextDecorations = TextDecorations.Underline;
                    ImageUpdateIcon.Source = 
                        new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Resources/update-available.png"))); 

                    foreach (var asset in root.GetProperty("assets").EnumerateArray())
                    {
                        string assetName = asset.GetProperty("name").GetString();
                        string downloadUrl = asset.GetProperty("browser_download_url").GetString();

                        if (assetName == GetCurrentBuildType())
                        { 
                            _latest = new UpdateRelease(releaseName, releaseNotes, assetName, latestVersion, downloadUrl);
                            break;
                        }

                    }
                }
                else
                {
                    TextUpdateStatus1.Text = "You have the latest version.";
                    ImageUpdateIcon.Source =
                        new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Resources/update-ok.png"))); 
                } 
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error checking for updates: {ex.Message}");
                TextUpdateStatus1.Text = "Error checking for updates!";
                ImageUpdateIcon.Source =
                        new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Resources/update-failed.png"))); 
                ToolTip.SetTip(TextUpdateStatus1, ex.Message);
            }
        }


        public void TextUpdateStatus1_Click(object sender, TappedEventArgs e)
        {
            if (_latest == null) return;
            var updateForm = new UpdateWindow(_latest);

            updateForm.ShowDialog(MainWindow.Instance);
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

        public void DonateButton_Click(object sender, TappedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.paypal.com/donate/?hosted_button_id=CSQHQU3DNCRYU") { UseShellExecute = true });
        }

        public void GitLink_Click(object sender, TappedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/leoaviana/WoWmapperX/releases") { UseShellExecute = true });
        }

        public void DonateButton_ImageReaction(object sender, PointerEventArgs e)
        {
            if (e.RoutedEvent.Name == "PointerEntered") 
                DonateButton.Source = new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Resources/donate-hover.png")));
            else
                DonateButton.Source = new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Resources/donate.png")));


        } 
    }
}
