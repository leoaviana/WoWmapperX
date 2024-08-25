using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using WoWmapperX.Keybindings;
using WoWmapperX.WorldOfWarcraft;
using WoWmapperX.Controllers;
using System.Collections.Generic;
using WoWmapperX.AvaloniaImpl;
using System.Threading.Tasks;

namespace WoWmapperX
{
    // load this application in a console window to use only the input mapping and/or injection
    // editing settings on this mode is not possible and the ingame overlay is not available.

    public static partial class Avalonialess
    { 
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        [LibraryImport("kernel32.dll")]
        private static partial IntPtr GetConsoleWindow();

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, [MarshalAs(UnmanagedType.Bool)] bool add);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        public static partial int NativeMessageBox(IntPtr hWnd, String text, String caption, uint type);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static readonly Guid _appGuid = System.Reflection.Assembly.GetExecutingAssembly().GetType().GUID;
        private static string[] Args;
        private static Mutex _mutex;
        private static bool loadLibraries = false;
        private static bool isShown = false;
        private static bool disableInput = false;
        private static bool allocConsole = true;
        private static List<string> librariesList;

        private delegate bool ConsoleEventDelegate(int eventType);

        static ConsoleEventDelegate handler;

        public static bool LoadLibraries { get { return loadLibraries; } }
        public static List<string> LibraryList { get { return librariesList; } }
        
        public static bool IsRunning { get; set; }  = false;

        public static void Initialize()
        {
            IsRunning = true;

            var commandLineArgs = Environment.GetCommandLineArgs();
            commandLineArgs.ToList().RemoveAt(0);
            Args = commandLineArgs.ToArray();
            
            if (Array.Find(Args, p => p == "-noconsole") != null)
            {
                allocConsole = false; 
            }

            if (allocConsole)
            {
                AllocConsole();

                handler = new ConsoleEventDelegate(ConsoleEventCallback);
                SetConsoleCtrlHandler(handler, true);
                isShown = true;
            }

            // ASCII Art: https://patorjk.com/software/taag/#p=display&f=Delta%20Corps%20Priest%201&t=WoWmapper
            Log.WriteLine("▄█     █▄   ▄██████▄   ▄█     █▄    ▄▄▄▄███▄▄▄▄      ▄████████    ▄███████▄    ▄███████▄    ▄████████    ▄████████ ", false);
            Log.WriteLine("███     ███ ███    ███ ███     ███ ▄██▀▀▀███▀▀▀██▄   ███    ███   ███    ███   ███    ███   ███    ███   ███    ███ ", false);
            Log.WriteLine("███     ███ ███    ███ ███     ███ ███   ███   ███   ███    ███   ███    ███   ███    ███   ███    █▀    ███    ███ ", false);
            Log.WriteLine("███     ███ ███    ███ ███     ███ ███   ███   ███   ███    ███   ███    ███   ███    ███  ▄███▄▄▄      ▄███▄▄▄▄██▀ ", false);
            Log.WriteLine("███     ███ ███    ███ ███     ███ ███   ███   ███ ▀███████████ ▀█████████▀  ▀█████████▀  ▀▀███▀▀▀     ▀▀███▀▀▀▀▀   ", false);
            Log.WriteLine("███     ███ ███    ███ ███     ███ ███   ███   ███   ███    ███   ███          ███          ███    █▄  ▀███████████ ", false);
            Log.WriteLine("███ ▄█▄ ███ ███    ███ ███ ▄█▄ ███ ███   ███   ███   ███    ███   ███          ███          ███    ███   ███    ███ ", false);
            Log.WriteLine(" ▀███▀███▀   ▀██████▀   ▀███▀███▀   ▀█   ███   █▀    ███    █▀   ▄████▀       ▄████▀        ██████████   ███    ███ ", false);
            Log.WriteLine("                                                                                                         ███    ███ ", false);
            Log.WriteLine("\n\n", false);
            Log.WriteLine("Avalonia UI has been disabled with the command line argument -nogui\n", false);
            if (Array.Find(Args, p => p == "-dinput") != null)
            {
                disableInput = true;
                Log.WriteLine("WoWmappers InputMapper has been disabled with the command line argument -dinput", false);
            }
            Log.WriteLine("Game overlay and configurations are unavailable in this mode.");
            Log.WriteLine("", false);

            Log.WriteLine("WoWmapper is now starting...");


            int tmp = Array.FindIndex<String>(Args, p => p.StartsWith("-ldlibraries:"));
            if (tmp != -1)
            {
                loadLibraries = true;
                librariesList = Args[tmp].Replace("-ldlibraries:", "").Split(',').ToList();
            }

            if (App.IsWine())
                Log.WriteLine("Wine environment detected. To avoid mouse and other input related issues, please try playing the game in windowed or " +
                    "windowed fullscreen modes.");
            try
            {
                OnStartup();
            } catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                Log.WriteLine("Stacktrace: " + ex.StackTrace);
            }
        }


        private static async void OnStartup()
        {
            if (!Args.Contains("--ignore-mutex")) // parameter to ignore existing instances
            {
                // Check if an existing instance of the application is running
                _mutex = new Mutex(false, "Global\\" + _appGuid);
                if (!_mutex.WaitOne(0, false))
                {
                    // Instance already running
                    NativeMessageBox(0, "Another instance of WoWmapper is already open", "Already running", (uint)(0x00000010L |  0x00000000L) );
                    Exit();
                    return;
                }
            }

            // Start up threads
            BindManager.LoadBindings();

            if (!disableInput)
            {
                ControllerManager.Start();
                InputMapper.Start();
            }

            ProcessManager.Start();

            await Task.WhenAny(ProcessManager.ManagerTask);

        }

        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
                Exit();
            return false;
        }
        private static void Exit()
        { 
            // Shut down threads
            if (!disableInput)
            {
                InputMapper.Stop();
                ControllerManager.Stop();
            }

            try
            {
                // Dispose mutex
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }
            catch { }


            ProcessManager.Stop();

            // System.Windows.Forms.Application.Exit(); 
        }
    }
}
