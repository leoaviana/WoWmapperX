using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace WoWmapperX.Controllers.DS4
{
    public class DS4Data
    {
        public static Bitmap GetButtonImage(GamepadButton button)
        {
            return ButtonImages[button];
        }

        public static string GetButtonName(GamepadButton button)
        {
            return ButtonNames[button];
        }

        private static Dictionary<GamepadButton, Bitmap> ButtonImages = new Dictionary<GamepadButton, Bitmap>
        {
            {
                GamepadButton.RFaceUp,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_R_UP.png")))
            },
            {
                GamepadButton.RFaceDown,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_R_DOWN.png")))
            },
            {
                GamepadButton.RFaceLeft,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_R_LEFT.png")))
            },
            {
                GamepadButton.RFaceRight,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_R_RIGHT.png")))
            },
            {
                GamepadButton.LFaceUp,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_L_UP.png")))
            },
            {
                GamepadButton.LFaceDown,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_L_DOWN.png")))
            },
            {
                GamepadButton.LFaceLeft,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_L_LEFT.png")))
            },
            {
                GamepadButton.LFaceRight,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_L_RIGHT.png")))
            },
                        {
                GamepadButton.LeftStickUp,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_L_UP.png")))
            },
            {
                GamepadButton.LeftStickDown,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_L_DOWN.png")))
            },
            {
                GamepadButton.LeftStickLeft,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_L_LEFT.png")))
            },
            {
                GamepadButton.LeftStickRight,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_L_RIGHT.png")))
            },
            {
                GamepadButton.ShoulderLeft,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_TL1.png")))
            },
            {
                GamepadButton.ShoulderRight,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_TR1.png")))
            },
            {
                GamepadButton.TriggerLeft,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_TL2.png")))
            },
            {
                GamepadButton.TriggerRight,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_TR2.png")))
            },
            {
                GamepadButton.CenterLeft,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_X_LEFT.png")))
            },
            {
                GamepadButton.CenterRight,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_X_RIGHT.png")))
            },
            {
                GamepadButton.CenterMiddle,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_X_CENTER.png")))
            },
            {
                GamepadButton.LeftStick,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_T_L3.png")))
            },
            {
                GamepadButton.RightStick,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_T_R3.png")))
            },
            {
                GamepadButton.TouchpadClick,
                new Bitmap(AssetLoader.Open(new Uri("avares://WoWmapperX/Controllers/DS4/Buttons/CP_R_UP.png")))
            },
        };

        private static Dictionary<GamepadButton, string> ButtonNames = new Dictionary<GamepadButton, string>
        {
            {GamepadButton.RFaceUp, "Triangle"},
            {GamepadButton.RFaceDown, "Cross"},
            {GamepadButton.RFaceLeft, "Square"},
            {GamepadButton.RFaceRight, "Circle"},
            {GamepadButton.LFaceUp, "D-pad Up"},
            {GamepadButton.LFaceDown, "D-pad Down"},
            {GamepadButton.LFaceLeft, "D-pad Left"},
            {GamepadButton.LFaceRight, "D-pad Right"},
            {GamepadButton.LeftStickUp, "Move Forward"},
            {GamepadButton.LeftStickDown, "Move Backward"},
            {GamepadButton.LeftStickLeft, "Move Left"},
            {GamepadButton.LeftStickRight, "Move Right"},
            {GamepadButton.ShoulderLeft, "L1"},
            {GamepadButton.ShoulderRight, "R1"},
            {GamepadButton.TriggerLeft, "L2"},
            {GamepadButton.TriggerRight, "R2"},
            {GamepadButton.CenterLeft, "Share"},
            {GamepadButton.CenterRight, "Options"},
            {GamepadButton.CenterMiddle, "PS"},
            {GamepadButton.LeftStick, "L3"},
            {GamepadButton.RightStick, "R3"},
            {GamepadButton.TouchpadClick, "Touchpad"},
        };
    }
}
