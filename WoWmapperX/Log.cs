using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WoWmapperX.AvaloniaImpl;

namespace WoWmapperX
{
    public static class Log
    {
        private static StreamWriter _file;

        public static void Initialize()
        {
            string defaultPath = "log.txt";
            string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWmapperX", "log.txt");

            try
            {
                _file = new StreamWriter(new FileStream(defaultPath, FileMode.Create)) { AutoFlush = true };
            }
            catch (UnauthorizedAccessException)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(userPath));
                _file = new StreamWriter(new FileStream(userPath, FileMode.Create)) { AutoFlush = true };
            }
        }

        public static void WriteLine(string text, params string[] args)
        {
            if (AppSettings.Default.EnableLogging)
            {
                lock(_file)
                    _file.WriteLine($"[{DateTime.Now.ToString("T")}] {text}", args);
            }

            Console.WriteLine($"[{DateTime.Now.ToString("T")}] {text}", args);
        }
        public static void WriteLine(string text, bool useDateTime = true, params string[] args)
        {
            if (AppSettings.Default.EnableLogging)
            {
                lock (_file)
                    if(useDateTime)
                        _file.WriteLine($"[{DateTime.Now.ToString("T")}] {text}", args);
                    else
                        _file.WriteLine(text, args);

            }

            if (useDateTime)
                Console.WriteLine($"[{DateTime.Now.ToString("T")}] {text}", args);
            else
                Console.WriteLine(text, args);

        }
    }
}
