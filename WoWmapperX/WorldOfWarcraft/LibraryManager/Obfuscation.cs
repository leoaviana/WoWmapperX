using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WoWmapperX.WorldOfWarcraft
{
    public static partial class Obfuscation
    {
        private const int Shift = 5;

        public static string DeobfuscateString(string str, ulong key)
        {
            byte[] keyBytes = BitConverter.GetBytes(key);
            char[] result = new char[str.Length];

            for (int i = 0; i < str.Length; i++)
            {
                int keyIndex = i % keyBytes.Length;
                int shiftedChar = str[i] - Shift - keyBytes[keyIndex] % 26;
                result[i] = (char)((shiftedChar < 32) ? shiftedChar + 95 : shiftedChar);
            }

            return new string(result);
        }
        public static IntPtr GetObfuscatedFunc(IntPtr module, string str, ulong key)
        {
            string newName = str;
            string deobfuscatedName = DeobfuscateString(newName, key);
            return GetProcAddress(module, deobfuscatedName);
        }


        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);
    }
}
