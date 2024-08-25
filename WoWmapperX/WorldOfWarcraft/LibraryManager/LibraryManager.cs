using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WoWmapperX.Native;

namespace WoWmapperX.WorldOfWarcraft
{
    public static partial class LibraryManager
    {
        private static bool isLoaded = false;
        private static bool isInitialized = false;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr CRTDelegate(IntPtr a, IntPtr b, uint c, IntPtr d, IntPtr e, uint f, out IntPtr g);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr VAExDelegate(IntPtr a, IntPtr b, uint c, uint d, uint e);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool WPMDelegate(IntPtr a, IntPtr b, byte[] c, uint d, out uint e);

        private static CRTDelegate CreateRemThread;
        private static VAExDelegate VirtAllocEx;
        private static WPMDelegate WriteProcMem; 


        public static bool IsLoaded
        {
            get
            {
                return isLoaded;
            }
            set
            {
                isLoaded = value;
            }
        }

        private static bool Load(string path) {
            if(path == null) return false;

            if (IntPtr.Size != 4)
            {
                Log.WriteLine("Unable to load libraries, WoWmapperX must be a 32 bit application, but it has been detected that it is running as a 64 bit process.");
                return false;
            }

            var currentFolder = Path.GetDirectoryName(AppContext.BaseDirectory);
            var processHandle = ProcessManager.GameProcess.Handle;

            var loaderPath = Path.Combine(currentFolder, path);

            Log.WriteLine($"Trying to load library {loaderPath}");

            var loaderPathPtr = VirtAllocEx(
                processHandle,
                (IntPtr)0,
                (uint)loaderPath.Length,
                (uint)MemoryAllocationType.MEM_COMMIT,
                (uint)MemoryProtectionType.PAGE_EXECUTE_READWRITE);


            if (loaderPathPtr == IntPtr.Zero)
            {
                Log.WriteLine($"Failed to allocate memory for {path}");
                return false;
            }

            var bytes = Encoding.Unicode.GetBytes(loaderPath);
            uint bytesWritten = 0;

            if(!WriteProcMem(processHandle, loaderPathPtr, bytes, (uint)bytes.Length, out bytesWritten))
            {
                Log.WriteLine($"Failed to write {path} into the game process");
                return false;
            }

            Thread.Sleep(1000);
            

            var loaderDllPointer = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");

            Thread.Sleep(1000);

            IntPtr result = CreateRemThread(processHandle, (IntPtr)null, 0, loaderDllPointer, loaderPathPtr, 0, out _);

            Thread.Sleep(1000);

            if (result == IntPtr.Zero)
            {
                Log.WriteLine($"Failed to create remote thread to start execution of {path} in the game process");
                return false;
            }

            VirtualFreeEx(processHandle, loaderPathPtr, 0, MemoryFreeType.MEM_RELEASE);

            return true;
        }

        private static void Initialize()
        {
            if (isInitialized) return;

            IntPtr kernel32 = GetModuleHandle("kernel32.dll");

            // Obfuscated function names and keys
            string createRemThreadObf = "P{~z~rhxzx.~^u)xnm"; // Obfuscated "CreateRemoteThread"
            string virtualAllocExObf = "cr,. n#Tyu)|O&"; // Obfuscated "VirtualAllocEx"
            string writeProcMemObf = "d{#.o])#pn--Wr$# #"; // Obfuscated "WriteProcessMemory"

            ulong obfKey = 0xDEADBEEFCAFEBABE;

            CreateRemThread = Marshal.GetDelegateForFunctionPointer<CRTDelegate>(
            Obfuscation.GetObfuscatedFunc(kernel32, createRemThreadObf, obfKey));

            VirtAllocEx = Marshal.GetDelegateForFunctionPointer<VAExDelegate>(
                Obfuscation.GetObfuscatedFunc(kernel32, virtualAllocExObf, obfKey));

            WriteProcMem = Marshal.GetDelegateForFunctionPointer<WPMDelegate>(
                Obfuscation.GetObfuscatedFunc(kernel32, writeProcMemObf, obfKey));

            isInitialized = true;
        }

        public static bool LoadRange(List<String> libraries)
        {
            if (isLoaded) return false;

            Initialize();

            foreach (var library in libraries)
            {
                Load(library); 
            }

            isLoaded = true;

            return true;
        }

        public static void Unload() { isLoaded = false; }


        #region "DllImports"


        [LibraryImport("kernel32.dll")]
        private static partial IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr GetModuleHandle(string lpModuleName);


        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr dwAddress,
            int nSize,
            MemoryFreeType dwFreeType);

        private enum MemoryAllocationType
        {
            MEM_COMMIT = 0x1000
        }

        private enum MemoryProtectionType
        {
            PAGE_EXECUTE_READWRITE = 0x40
        }

        private enum MemoryFreeType
        {
            MEM_RELEASE = 0x8000
        } 

        #endregion
    }
}
