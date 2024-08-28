using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using WoWmapperX.AvaloniaImpl;
using WoWmapperX.WorldOfWarcraft;
using System.Runtime.InteropServices;

//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WoWmapperX
{
    partial class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break. 


        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();


        [STAThread]
        public static void Main(string[] args)
        {
#if DEBUG
            // Debugger.Launch();
#endif

            Log.Initialize();

            if (args.Contains("--help") || args.Contains("-h"))
            {
                AllocConsole();

                Console.WriteLine("Available commands:\n");
                Console.WriteLine("-h / --help                 : displays information for all available command line arguments");
                Console.WriteLine("-rg:path_to_wow.exe         : launches the game process given it's path eg.: -rg:C:\\Wow\\Wow.exe");
                Console.WriteLine("-rg:path_to_wow.exe,(-c;-d) : launches the game process given it's path and with command line arguments separated by ;");
                Console.WriteLine("-term                       : terminate WoWmapperX whenever the first game process detected terminates (game processes are named Wow.exe)"); 
                Console.WriteLine("-dterm                      : terminate WoWmapperX whenever the specified process given by -rg terminates");
                Console.WriteLine("-nogui                      : launch in console mode, useful for logging, verbose mode and lightweight environments");
                Console.WriteLine("-noconsole                  : launch in silent mode. only available if used with -nogui");

                Console.WriteLine("Examples:\n");
                Console.WriteLine("Run WoWmapperX.exe silently and start game using it's path making sure to close whenever game process finishes: WoWmapperX.exe -nogui -noconsole -rg:C:\\Wow.exe -dterm");
                Console.WriteLine("Run WoWmapperX.exe silently and start game using a launcher's path making sure to close whenever the first detected game process finishes: WoWmapperX.exe -nogui -noconsole -rg:C\\WoWLauncher.exe -dterm");
                Console.WriteLine("Run WoWmapperX.exe silently and start game using LibConsole (WoWCamera.exe) with command line arguments making sure to close whenever the first detected game process finishes: WoWmapperX.exe -nogui -noconsole -rg:C\\WoWCamera.exe,(-n:Wow.exe) -dterm");

                Console.WriteLine("\n\nPress any key to exit.");
                Console.Read();

                return;
            }

            foreach (string arg in args)
            {
                if (arg.Contains("-rg:"))
                {
                    var process = arg.Replace("-rg:", "").Split(',');
                    var procName = process[0];
                    string procArgs = "";

                    if (process.Length > 1)
                    {
                        procArgs = process[2].Replace("(", "").Replace(")", "").Replace(';', ' ');
                    }

                    try
                    {
                        if (args.Contains("-dterm"))
                        {
                            ProcessManager.SetGameProcess(System.Diagnostics.Process.Start(procName, procArgs));
                        }
                        else
                            System.Diagnostics.Process.Start(procName, procArgs);

                        break;
                    }
                    catch
                    {
                        return;
                    }
                }
            }

            if(args.Contains("-term") || args.Contains("-dterm"))
            {
                App.WaitForExit = true;
            }

            if (!args.Contains("-nogui"))
                BuildAvaloniaApp().StartWithClassicDesktopLifetimeCustom(args);
            else
                Avalonialess.Initialize();
        }


        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
} 

namespace Avalonia
{
    public static class ClassicDesktopStyleApplicationLifetimeExtensions
    {
        public static int StartWithClassicDesktopLifetimeCustom(
           this AppBuilder builder, string[] args, Avalonia.Controls.ShutdownMode shutdownMode = Avalonia.Controls.ShutdownMode.OnLastWindowClose)
        {
            var lifetime = new ClassicDesktopStyleApplicationLifetime()
            {
                Args = args,
                ShutdownMode = (Controls.ShutdownMode)shutdownMode
            };
            builder.SetupWithLifetime(lifetime);
            return lifetime.CustomStart(args); 
        }

        public static int CustomStart<T>(
            this T builder, string[] args)
            where T : Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime, new()
        {
            var wnd = builder.MainWindow;
            builder.MainWindow = null;

            Task.Run(async () => 
            {
                // I couldn't find a better way of doing this.

                await Task.Delay(5);               

                builder.MainWindow = wnd;

                if ((AppSettings.Default.HideAtStartup && AppSettings.Default.RunInBackground) || Array.Exists(args, x => x.Contains("-rg:")))
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => builder.MainWindow?.Hide());
                else
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => builder.MainWindow?.Show());
            });

            return builder.Start(args); 
        }
    }
}