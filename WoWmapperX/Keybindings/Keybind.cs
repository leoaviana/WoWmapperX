using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks; 
using System.Xml.Serialization;
using WoWmapperX.AvaloniaImpl;
using WoWmapperX.Controllers;

namespace WoWmapperX.Keybindings
{
    public class Keybind
    {
        public GamepadButton BindType { get; set; }

        public Key Key { get; set; }
    }

    public class Keybinds
    {
        public List<Keybind> Bindings { get; set; }

        public Keybinds()
        {
            Bindings = new();
        }

    }

    [JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonSerializable(typeof(Keybinds), GenerationMode = JsonSourceGenerationMode.Metadata)]
    internal partial class KeybindsGenerationContext : JsonSerializerContext
    {
    }

}