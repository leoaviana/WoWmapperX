using Avalonia;
using Avalonia.Input;
using System;
using System.Runtime.InteropServices;

namespace WoWmapperX.AvaloniaImpl
{
    public partial class Cursor
    {
        // P/Invoke constants
        private const int MOUSEEVENTF_MOVE = 0x0001;

        // P/Invoke declarations
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetCursorPos(out POINT lpPoint);

        [LibraryImport("user32.dll")]
        private static partial void SetCursorPos(int x, int y);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        // Properties
        public static Point Position
        {
            get
            {
                if (GetCursorPos(out POINT point))
                {
                    return new Point(point.X, point.Y);
                }
                else
                {
                    throw new InvalidOperationException("Failed to get cursor position.");
                }
            }
            set
            {
                SetCursorPos((int)value.X, (int)value.Y);
            }
        }

        public static int X => (int)Position.X;
        public static int Y => (int)Position.Y;
    }
}
