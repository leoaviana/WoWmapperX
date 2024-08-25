using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using WoWmapperX.AvaloniaImpl;

namespace WoWmapperX.WoWInfoReader
{
    public static partial class WoWReader
    {
        private static IntPtr _handle;
        public static IntPtr offsetAoeState;
        public static IntPtr offsetLoggedState;
        public static IntPtr offsetGameState;
        public static IntPtr offsetMouselookState;
        public static IntPtr offsetPlayerBase;
        public static IntPtr offsetWalkState;
        private static Process _process;
        private static bool _offsetsLoaded;

        public static bool IsAttached => AppSettings.Default.EnableMemoryReading &&
                                         (_handle != IntPtr.Zero) &&
                                         (_process != null) && _offsetsLoaded;

        public static Tuple<long, long> PlayerHealth => ReadPlayerHealth();

        public static bool MouselookState => ReadMouselook();

        public static bool GameState => ReadGameState();

        public static bool LoggedState => ReadAccountLoggedIn();

        public static int MovementState => ReadMovementState();

        public static bool AoeState => ReadAoeState();

        public static void Open(Process wowProcess)
        {
            if (IsAttached) Close();

            // Test process architecture
            //bool isWin32;
            //IsWow64Process(wowProcess.Handle, out isWin32);

            //if (!isWin32)
            //   throw new Exception("The selected process must be 32-bit.");

            // Attempt to open for memory reading
            var hProc = OpenProcess(ProcessAccessFlags.VirtualMemoryRead, false, wowProcess.Id);
            if (hProc == IntPtr.Zero)
                throw new Exception("Unable to open process for memory reading.");
            
            _process = wowProcess;
            _handle = hProc;

            LoadOffsets(wowProcess);
        }

        public static void Clear()
        {
            offsetMouselookState = IntPtr.Zero;
            offsetWalkState = IntPtr.Zero;
            offsetAoeState = IntPtr.Zero;
            offsetGameState  = IntPtr.Zero;
            offsetPlayerBase = IntPtr.Zero;
            _offsetsLoaded = false;
        }

        public static void Close()
        {
            Clear();
            CloseHandle(_handle);
            _handle = IntPtr.Zero;
            _process = null;
        }

        private static void LoadOffsets(Process process)
        {
            var scanner = new SigScan(process, process.MainModule.BaseAddress,
                process.MainModule.ModuleMemorySize);

            try
            {
                offsetGameState = scanner.FindPattern(OffsetPattern.GameState.Pattern);
                if (offsetGameState == IntPtr.Zero) throw new Exception("Unable to match pattern for GameState");
                offsetGameState = ReadPointer(offsetGameState + OffsetPattern.GameState.Offset);

                
                offsetAoeState = scanner.FindPattern(OffsetPattern.AoeState.Pattern);
                if (offsetAoeState == IntPtr.Zero) throw new Exception("Unable to match pattern for AoeState");
                offsetAoeState = ReadPointer(offsetAoeState + OffsetPattern.AoeState.Offset);

                
                offsetMouselookState = scanner.FindPattern(OffsetPattern.MouselookState.Pattern);
                if (offsetMouselookState == IntPtr.Zero)
                    throw new Exception("Unable to match pattern for MouselookState");
                offsetMouselookState = ReadPointer(offsetMouselookState + OffsetPattern.MouselookState.Offset);



                // Could not find SigScans for the player's health the other stuff below. Using pointers instead


                // Not really playerbase, it's just a pointer for the main char health.
                int[] PlayerBaseOffsets = { 0x34, 0x24, 0x4dc, 0x3e8, 0x6ec };
                offsetPlayerBase = ReadPointer((IntPtr)(process.MainModule.BaseAddress.ToInt64() + 0x008D87A8), PlayerBaseOffsets);


                int[] WalkStateOffsets = { 0x34, 0x24, 0x578, 0x4, 0x254 };
                offsetWalkState = ReadPointer((IntPtr)(process.MainModule.BaseAddress.ToInt64() + 0x008D87A8), WalkStateOffsets);


                // using pointer also for the account logged state.

                int[] LoggedStateOffsets = { 0x2f3c };
                offsetLoggedState = ReadPointer((IntPtr)(process.MainModule.BaseAddress.ToInt64() + 0x879CE0), LoggedStateOffsets);


                _offsetsLoaded = true;
                Log.WriteLine($"Offset scan was successful!\n" +
                              $"GameState:      {offsetGameState.ToString("X2")}\n" +
                              $"AoeState:       {offsetAoeState.ToString("X2")}\n" +
                              $"MouselookState: {offsetMouselookState.ToString("X2")}\n" +
                              $"WalkState:      {offsetWalkState.ToString("X2")}\n" +
                              $"PlayerBase:     {offsetPlayerBase.ToString("X2")}\n" +
                              $"LoginState:     {offsetLoggedState.ToString("X2")}\n");

            }
            catch (Exception ex)
            {
                Log.WriteLine("Offset scan failed: " + ex.Message);
            }
            
        }


        private static void UpdatePlayerBasePointer()
        {
            try {  
                // Not really playerbase, it's just a pointer for the main char health.
                int[] PlayerBaseOffsets = { 0x34, 0x24, 0x4dc, 0x3e8, 0x6ec };
                var newOffsetPlayerBase = ReadPointer((IntPtr)(_process.MainModule.BaseAddress.ToInt64() + 0x008D87A8), PlayerBaseOffsets); 

                if(offsetPlayerBase != newOffsetPlayerBase)
                {
                    offsetPlayerBase = newOffsetPlayerBase;
                }

            }
            catch (Exception ex)
            {
                Log.WriteLine("Pointer update failed: " + ex.Message);
            }
        }


        private static void UpdateWalkStatePointer()
        {
            try
            {

                int[] WalkStateOffsets = { 0x34, 0x24, 0x578, 0x4, 0x254 };
                var newOffsetWalkState = ReadPointer((IntPtr)(_process.MainModule.BaseAddress.ToInt64() + 0x008D87A8), WalkStateOffsets);

                if (offsetWalkState != newOffsetWalkState)
                {
                    offsetWalkState = newOffsetWalkState; 
                }

            }
            catch (Exception ex)
            {
                Log.WriteLine("Pointer update failed: " + ex.Message);
            }
        }

        private static void UpdateLoggedStatePointer()
        {
            try
            {

                int[] LoggedStateOffsets = { 0x2f3c };
                var newOffsetLoggedState = ReadPointer((IntPtr)(_process.MainModule.BaseAddress.ToInt64() + 0x879CE0), LoggedStateOffsets);

                if (offsetLoggedState != newOffsetLoggedState)
                {
                    offsetLoggedState = newOffsetLoggedState; 
                }

            }
            catch (Exception ex)
            {
                Log.WriteLine("Pointer update failed: " + ex.Message);
            }
        }

        private static Tuple<long, long> ReadPlayerHealth()
        {
            if (!IsAttached || !_offsetsLoaded) return null;
            try
            {
                UpdatePlayerBasePointer();
                var playerBase = Read<int>(offsetPlayerBase);
                var maxHealth = Read<int>(offsetPlayerBase + 0xA28);
                return new Tuple<long, long>(playerBase, maxHealth);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                return null;
            }
        }

        private static bool ReadMouselook()
        {
            if (!IsAttached || !_offsetsLoaded) return false;
            try
            {
                var mouselookState = Read<byte>(offsetMouselookState);

                return mouselookState == 1;
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                return false;
            }
        }

        private static byte lastState = 0;
        private static bool ReadGameState()
        {
            if (!IsAttached || !_offsetsLoaded) return false;

            try
            {
                var gameState = Read<byte>(offsetGameState); 
                if(gameState != 0 && gameState != 1) Log.WriteLine($"GameState unexpected value: {gameState}");
                if (lastState != gameState)
                {
                    Log.WriteLine($"GameState changed: {gameState}");
                    lastState = gameState;
                }
                return gameState != 0;
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                return false;
            }
        }


        private static byte lastLState = 0;
        private static bool ReadAccountLoggedIn()
        {
            if (!IsAttached || !_offsetsLoaded) return false;

            try
            {
                UpdateLoggedStatePointer();
                var loggedState = Read<byte>(offsetLoggedState);

                if (loggedState != 0 && loggedState != 1) Log.WriteLine($"LoggedState unexpected value: {loggedState}");
                if (lastLState != loggedState)
                {
                    Log.WriteLine($"LoggedState changed: {loggedState}");
                    lastLState = loggedState;
                } 

                return loggedState != 0;
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                return false;
            }
        }

        private static int ReadMovementState()
        {
            if (!IsAttached || !_offsetsLoaded) return 0;
            try
            {
                UpdateWalkStatePointer();

                var walkState = Read<byte>(offsetWalkState);

                return walkState;
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                return 0;
            }
        }

        private static bool ReadAoeState()
        {
            if (!IsAttached || !_offsetsLoaded) return false;
            try
            {
                var aoeState = Read<byte>(offsetAoeState);

                return aoeState == 2;
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                return false;
            }
        }

        private static T Read<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(IntPtr offset)
        {
            var typeLength = Marshal.SizeOf<T>();
            var bytes = new byte[typeLength];
            IntPtr bytesRead;

            var readSuccess = ReadProcessMemory(_handle, offset, bytes, bytes.Length, out bytesRead);

            if (!readSuccess) return default(T);

            var typePtr = Marshal.AllocHGlobal(bytes.Length);
            try
            {
                Marshal.Copy(bytes, 0, typePtr, bytes.Length);
                var returnType = Marshal.PtrToStructure<T>(typePtr);
                return returnType;
            }
            finally
            {
                Marshal.FreeHGlobal(typePtr);
            }
        }

        private static IntPtr ReadPointer(IntPtr startAddress, IReadOnlyList<int> pointerOffsets, int staticOffset = 0)
        {
            var currentAddress = startAddress;

            foreach (var pointerOffset in pointerOffsets)
            {
                var pointer = Read<int>(currentAddress) + pointerOffset; 
                currentAddress = (IntPtr)pointer; 
            }

            return currentAddress + staticOffset;
        }

        private static IntPtr ReadPointer(IntPtr startAddress, int staticOffset = 0)
        {
            return ReadPointer(startAddress, new int[1], staticOffset);
        }

        #region Native Methods and Enums

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool IsWow64Process(in IntPtr processHandle,
            [MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            int processId
        );

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(IntPtr hObject);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        #endregion
    }
}