using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls; 
using Avalonia.Markup.Xaml; 
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using WoWmapperX.Controllers;
using WoWmapperX.Keybindings;
using WoWmapperX.Overlay;
using WoWmapperX.Views;
using WoWmapperX.WorldOfWarcraft;
using WoWmapperX.Native;  
using System.Reactive.Linq;
using WoWmapperX.AvaloniaImpl;
using ReactiveUI;
using System.Reactive;
using WoWmapperX.ViewModels;
using System.Collections.Generic;

namespace WoWmapperX
{
    public partial class App : Application
    {

        [LibraryImport("ntdll.dll")] 
        private static partial IntPtr wine_get_version();

        private static string GetWineVersion()
        {
            return Marshal.PtrToStringAuto(wine_get_version());
        }


        internal static WMOverlay Overlay = new();

        private readonly Guid _appGuid = System.Reflection.Assembly.GetExecutingAssembly().GetType().GUID;
        private static Mutex _mutex;
        private static bool loadLibraries = false;
        private static List<string> librariesList;

        public static bool WaitForExit { get; set; } = false;
        public static bool LoadLibraries { get { return loadLibraries; } }
        public static List<string> LibraryList { get { return librariesList; } }

        public readonly string[] Args; 


        public delegate void themeChanged(object sender, EventArgs e);
        public static event themeChanged CurrentThemeChanged;


        public App()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            commandLineArgs.ToList().RemoveAt(0);
            Args = commandLineArgs.ToArray();

            AppSettings.Default.PropertyChanged += (sender, args) =>
            {
                AppSettings.Default.Save();
            };
        }

        public static void InvokeThemeChanged()
        {
            CurrentThemeChanged?.Invoke(null, null);
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            this.DataContext = new TrayIconViewModel();
            OnStartup();
        }
        public static bool IsWine()
        {
            try
            {
                var version = GetWineVersion();
                return true;
            }
            catch
            {
            };

            return false;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            try
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {

                    desktop.MainWindow = new MainWindow
                    {
                    };

                    if(IsWine())
                    {
                        // avoid window compositor creating another titlebar, not needed on windows.

                        var style = NativeMethods.GetWindowLongPtr(desktop.MainWindow.TryGetPlatformHandle().Handle, -16);
                        style = (IntPtr)(style.ToInt64() & ~0x00C00000L);
                        NativeMethods.SetWindowLongPtr(desktop.MainWindow.TryGetPlatformHandle().Handle, -16, style);
                    } 

                    desktop.Exit += OnExit;
                }

                base.OnFrameworkInitializationCompleted();
            }
            catch (Exception ex)
            { 
                Log.WriteLine(ex.Message); 
                ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime).Shutdown(1);
            }
        }

        private async void OnStartup()
        {
            
            if (!Args.Contains("--ignore-mutex")) // parameter to ignore existing instances
            {
                // Check if an existing instance of the application is running
                _mutex = new Mutex(false, "Global\\" + _appGuid);
                if (!_mutex.WaitOne(0, false))
                {

                    await MessageBox.Show("Another instance of WoWmapper is already open", "Already running", MessageBoxButton.OK);

                    Environment.Exit(0);
                    return;
                }

            }

            // Start up threads
            BindManager.LoadBindings();
            ProcessManager.Start();
            ControllerManager.Start();
            InputMapper.Start();
        } 
        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            if (Design.IsDesignMode) return;

            // Shut down threads
            ProcessManager.Stop();
            InputMapper.Stop();
            ControllerManager.Stop();
            Overlay.Stop();


            // Save settings
            AppSettings.Default.Save();

            try
            {
                // Dispose mutex
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }
            catch { }

        }
    }
}
