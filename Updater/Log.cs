using System.Diagnostics;

namespace Updater
{
    public static class Log
    {
        private static StreamWriter _file;

        public static void Initialize()
        {
            string defaultPath = "update_log.txt";
            string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWmapperX", "update_log.txt");

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
            lock(_file)
                _file.WriteLine($"[{DateTime.Now.ToString("T")}] {text}", args); 

            Console.WriteLine($"[{DateTime.Now.ToString("T")}] {text}", args);
            Debug.WriteLine($"[{DateTime.Now.ToString("T")}] {text}", args);
        }
    }
}
