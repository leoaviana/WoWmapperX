using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WoWmapperX.WoWInfoReader;
using ThreadState = System.Threading.ThreadState;
using WoWmapperX.AvaloniaImpl;

namespace WoWmapperX.WorldOfWarcraft
{
    internal static partial class ProcessManager
    {
        #region Natives

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left; // x position of upper-left corner
            public int Top; // y position of upper-left corner
            public int Right; // x position of lower-right corner
            public int Bottom; // y position of lower-right corner
        }

        #endregion

        private static readonly string[] WoWProcessNames = new[] {"wow", "wow-64", "wowt", "wowt-64", "wowb", "wowb-64"}; 
        private static CancellationTokenSource _cts;

        public static Task ManagerTask { get; private set; }
        public static Process GameProcess { get; private set; }
        public static bool GameRunning => GameProcess != null;

        private struct ProcInfo
        {
            public int Id;
            public string ProcessName; 
        }

        private static ProcInfo wGameProcessInfo = new ProcInfo();

        public static Rectangle GetClientRectangle()
        {
            RECT clientRect, windowRect;
            GetClientRect(GameProcess.MainWindowHandle, out clientRect);
            GetWindowRect(GameProcess.MainWindowHandle, out windowRect);

            var borderWidth = (windowRect.Right - windowRect.Left - clientRect.Right)/2;
            var titleHeight = (windowRect.Bottom - windowRect.Top) - clientRect.Bottom - borderWidth;

            var outRectangle = new Rectangle(windowRect.Left + borderWidth, windowRect.Top + titleHeight,
                clientRect.Right, clientRect.Bottom);
            return outRectangle;
        }

        private static void ProcessThreadMethod(CancellationToken token)
        {
            // Process watcher thread
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (GameProcess == null)
                {
                    try
                    {
                        // Acquire a list of all processes
                        var wowProcess =
                            Process.GetProcesses()
                                .FirstOrDefault(
                                    process =>
                                        WoWProcessNames.Contains(process.ProcessName.ToLower()) &&
                                        process.HasExited == false);

                        if (wowProcess != null)
                        {
                            GameProcess = wowProcess;

                            //caching both of these infos just in case we refresh the process after it's been terminated.
                            wGameProcessInfo.Id = wowProcess.Id;
                            wGameProcessInfo.ProcessName = wowProcess.ProcessName;

                            Log.WriteLine($"Found game process: [{GameProcess.Id}: {GameProcess.ProcessName}]"); 

                            // Attempt to export bindings
                            ConsolePort.BindWriter.WriteBinds();

                            if (AppSettings.Default.EnableMemoryReading) // Attach memory reader
                                WoWReader.Open(wowProcess);

                            if (Avalonialess.IsRunning)
                            {
                                if (Avalonialess.LoadLibraries == true && !LibraryManager.IsLoaded)
                                    LibraryManager.LoadRange(Avalonialess.LibraryList);
                            }
                            else
                                if (App.LoadLibraries == true && !LibraryManager.IsLoaded)
                                    LibraryManager.LoadRange(App.LibraryList);

                            if(Environment.GetCommandLineArgs().Contains("-dterm"))
                            {
                                if (Avalonialess.IsRunning)
                                    _cts.Cancel();
                                else
                                    Avalonia.Threading.Dispatcher.UIThread.Post(() => (Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(0));

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine($"Exception occurred: {ex.Message}");
                    }
                }

                try
                {

                    if (GameProcess != null)
                    {
                        // Test process validity
                        if (GameProcess.HasExited)
                        {
                            Log.WriteLine($"Process [{wGameProcessInfo.Id}: {wGameProcessInfo.ProcessName}] has exited");

                            LibraryManager.Unload();

                            GameProcess.Dispose();
                            GameProcess = null;

                            if (App.WaitForExit == true)
                                Avalonia.Threading.Dispatcher.UIThread.Post(() => (Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(0));

                            continue;
                        }

                        // whenever a windowstate is set under wine, the main window handle changes, and looks like this isn't supposed
                        // to happen (at least doesn't happen on Windows) because on .NET, the Process information is cached, this solution seems to work.
                        if (App.IsWine())
                            if (GameProcess != null)
                                GameProcess.Refresh();

                        // Attach/detach memory reader as necessary
                        if (AppSettings.Default.EnableMemoryReading && !WoWReader.IsAttached)
                            WoWReader.Open(GameProcess);

                        if (!AppSettings.Default.EnableMemoryReading && WoWReader.IsAttached)
                            WoWReader.Close();

                        if (!LibraryManager.IsLoaded) 
                        {
                            if (Avalonialess.IsRunning)
                            {
                                if (Avalonialess.LoadLibraries == true && !LibraryManager.IsLoaded)
                                    LibraryManager.LoadRange(Avalonialess.LibraryList);
                            }
                            else
                                if (App.LoadLibraries == true && !LibraryManager.IsLoaded)
                                    LibraryManager.LoadRange(App.LibraryList);

                            if (Environment.GetCommandLineArgs().Contains("-dterm"))
                            {
                                if (Avalonialess.IsRunning)
                                    _cts.Cancel();
                                else
                                    Avalonia.Threading.Dispatcher.UIThread.Post(() => (Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(0));

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"Exception occurred (1): {ex.Message}");
                }


                Thread.Sleep(500);
            }
        }

        internal static void SetGameProcess(Process process)
        {
            GameProcess = process;
        }

        internal static void Start()
        {
            _cts = new CancellationTokenSource();
            ManagerTask = Task.Run(() => ProcessThreadMethod(_cts.Token), _cts.Token);
        }

        internal static void Stop()
        {
            WoWReader.Close();
            _cts.Cancel() ;
        }
    }
    internal enum ProcessType
    {
        None,
        WoW32,
        WoW64,
        WoWT,
        WoWT64
    }
}