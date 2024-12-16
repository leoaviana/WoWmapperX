using System.Diagnostics;
using System.IO.Compression;

namespace Updater
{
    internal class Program
    { 
        private static string _updateArchive;

        static void Main(string[] args)
        {
            Log.Initialize(); 
            
            if(args.Length < 1)
            {
                Log.WriteLine("Updater started without a specified update file, exiting...");
                Environment.Exit(0);
            }

            try
            {
                var argFilename = args[0];

                if (!File.Exists(argFilename))
                {
                    Environment.Exit(0);
                    return;
                }
                _updateArchive = argFilename;

                Task.Run(async () => await ProcessUpdate()).Wait();
                
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }

        public static async Task ProcessUpdate()
        {
            Log.WriteLine("Waiting for WoWmapperX to exit...");
            await Task.Run(() =>
            {
                while (true)
                {
                    var proc = Process.GetProcessesByName("wowmapperx.exe");
                    if (proc.Length == 0) break;
                }
            });

            await Task.Run(() => Thread.Sleep(2000));

            Log.WriteLine("Extracting files...");
            await Task.Run(() =>
            {
                var update = ZipFile.OpenRead(_updateArchive);
                foreach (var file in update.Entries)
                {
                    if (file.Name == "WoWmapperX_Updater.exe" || string.IsNullOrEmpty(Path.GetFileName(file.Name))) continue;
                    var destDir = Path.GetDirectoryName(file.FullName);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                    try
                    {
                        file.ExtractToFile(file.FullName, true);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine($"An error occurred during extraction. You may need to update manually.\n\n{ex.Message}");
                        return;
                    }
                }
                update.Dispose();
                File.Delete(_updateArchive);

                Log.WriteLine("Update done, starting WoWmapperX.exe...");

                Process.Start("WoWmapperX.exe");
            });
            Environment.Exit(0);
        }
    }
}
