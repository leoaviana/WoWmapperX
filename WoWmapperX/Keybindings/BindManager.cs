using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WoWmapperX.ConsolePort;
using WoWmapperX.Controllers;
using WoWmapperX.AvaloniaImpl;
using System.Text.Json;

namespace WoWmapperX.Keybindings
{
    public static class BindManager
    {
        public static event EventHandler BindingsChanged;
        public static readonly string KeybindFile = "keybinds.json";
        private static readonly string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWmapperX", KeybindFile);


        public static List<Keybind> CurrentKeybinds = new List<Keybind>();

        public static Key GetKey(GamepadButton button)
        {
            try
            {
                if (AppSettings.Default.CustomBindings)
                {
                    return CurrentKeybinds.First(
                        bind => bind.BindType == button).Key;
                }
                else
                {
                    return
                        Defaults.KeybindStyles.GetDefault(AppSettings.Default.ModifierStyle).First(
                            bind => bind.BindType == button).Key;
                }
            }
            catch
            {
                return Key.None;
            }
        }

        public static void SetKey(GamepadButton button, Key key)
        {
            var binding = CurrentKeybinds.First(bind => bind.BindType == button);
            binding.Key = key;

            SaveBindings();
            BindingsChanged?.Invoke(null, EventArgs.Empty);

            AppSettings.Default.BindingsModified = DateTime.Now;
            BindWriter.WriteBinds();
        }

        private static string ReadKeybindsFile()
        {
            try
            {
                return File.ReadAllText(KeybindFile);
            }
            catch (FileNotFoundException)
            {
                try
                {
                    return File.ReadAllText(userPath);
                }
                catch
                {
                    return null;
                }
            }
            catch (UnauthorizedAccessException)
            {
                try
                {
                    return File.ReadAllText(userPath);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static void LoadBindings()
        {
            if (!File.Exists(KeybindFile) && !File.Exists(userPath)) ResetDefaults(0);

            /*
            // Load keybinds from file
            var xml = new XmlSerializer(typeof (List<Keybind>));

            using (var fs = new FileStream(KeybindFile, FileMode.Open))
                CurrentKeybinds = (List<Keybind>) xml.Deserialize(fs);
            */

            var kbinds = new Keybinds();

            try
            {
                var json = ReadKeybindsFile();
                kbinds = JsonSerializer.Deserialize<Keybinds>(json, KeybindsGenerationContext.Default.Keybinds);
                CurrentKeybinds = kbinds.Bindings; 
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error loading keybinds settings: {ex.Message}");
                ResetDefaults(0);
            }
        }

        public static void SaveBindings()
        {
            /*
            // Save keybinds to file
            var xml = new XmlSerializer(typeof (List<Keybind>));
            using (var fs = new FileStream(KeybindFile, FileMode.Create))
                xml.Serialize(fs, CurrentKeybinds);
            */

            var kbinds = new Keybinds();
            kbinds.Bindings = CurrentKeybinds;

            var json = JsonSerializer.Serialize(kbinds, KeybindsGenerationContext.Default.Keybinds);
            try
            {
                try
                {
                    File.WriteAllText(KeybindFile, json);
                }
                catch (UnauthorizedAccessException)
                {
                    File.WriteAllText(userPath, json);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error saving keybinds settings: {ex.Message}");
            }
        }

        public static void ResetDefaults(int profile)
        {
            CurrentKeybinds.Clear();
            CurrentKeybinds.AddRange(Defaults.KeybindStyles.GetDefault(AppSettings.Default.ModifierStyle));
            
            AppSettings.Default.ModifierStyle = profile;
            SaveBindings();
            BindingsChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}