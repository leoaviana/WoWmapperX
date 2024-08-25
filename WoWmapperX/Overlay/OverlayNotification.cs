using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace WoWmapperX.Overlay
{
    public class OverlayNotification
    {
        public string Header { get; set; }
        public string Content { get; set; }
        public Bitmap Image { get; set; }
        public int Duration { get; set; } = 5000;
        public int FadeIn { get; set; } = 250;
        public int FadeOut { get; set; } = 750;
        public string UniqueID { get; set; } = null;
    }

    public static class OverlayIcons
    {
        public static Bitmap Rhonin = new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Overlay/Images/Rhonin.png")));
        public static Bitmap Illidan = new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Overlay/Images/Illidan.png")));

        private static Random _rnd = new Random();

        public static Bitmap Random()
        {
            var rand = _rnd.Next(0, 2);
            switch (rand)
            {
                case 0:
                    return Rhonin;
                case 1:
                    return Illidan;
            }
            return Illidan;
        }
        
    }
}
