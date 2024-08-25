using Avalonia.Input;
using System;
using System.Runtime.InteropServices;
using System.Threading; 
using WoWmapperX.Input;
using WoWmapperX.Keybindings;
using WoWmapperX.Overlay;
using WoWmapperX.WorldOfWarcraft;
using WoWmapperX.WoWInfoReader;
using WoWmapperX.AvaloniaImpl;
using Cursor = WoWmapperX.AvaloniaImpl.Cursor;
using Point = Avalonia.Point;

namespace WoWmapperX.Controllers
{
    public static partial class InputMapper
    {
        private static readonly Thread _inputThread = new Thread(InputWatcherThread);
        private static readonly bool[] _keyStates = new bool[Enum.GetNames(typeof (GamepadButton)).Length];
        private static bool _threadRunning;
        private static readonly HapticFeedback HapticFeedback = new HapticFeedback();
        private static DateTime _mouselookStarted;
        private static bool _setMouselook;
        private static bool _stopWalk;
        private static int _cursorX;
        private static int _cursorY;
        private static bool _crosshairShowing;

        static InputMapper()
        {
            ControllerManager.ControllerButtonStateChanged += ActiveController_ButtonStateChanged;
        }

        [LibraryImport("user32.dll")]
        private static partial IntPtr GetForegroundWindow();

        private static void InputWatcherThread()
        {
            while (_threadRunning)
            {
                var axisMovement = AppSettings.Default.SwapSticks
                    ? ControllerManager.GetRightAxis()
                    : ControllerManager.GetLeftAxis();
                var axisCursor = AppSettings.Default.SwapSticks
                    ? ControllerManager.GetLeftAxis()
                    : ControllerManager.GetRightAxis();

                ProcessMovement(axisMovement);

                ProcessCursor(axisCursor);


                if (ProcessManager.GameProcess != null &&
                    WoWReader.IsAttached && WoWReader.GameState)
                {
                    var foregroundWindow = GetForegroundWindow();
                    if (foregroundWindow == ProcessManager.GameProcess?.MainWindowHandle)
                        _setMouselook = false;

                    // Cancel mouselook when alt-tabbed
                    if (AppSettings.Default.MemoryAutoCancel && !_setMouselook && WoWReader.MouselookState &&
                        foregroundWindow != ProcessManager.GameProcess?.MainWindowHandle)
                    {
                        WoWInput.SendMouseClick(MouseButton.Right, true);
                        _setMouselook = true;
                    }

                    // Show/hide the overlay crosshair
                    if (AppSettings.Default.EnableOverlay && AppSettings.Default.EnableOverlayCrosshair)
                    {
                        // Show crosshair after mouselooking for 100ms
                        if (WoWReader.MouselookState && DateTime.Now >= _mouselookStarted + TimeSpan.FromMilliseconds(200) &&
                            !App.Overlay.CrosshairVisible && !_crosshairShowing)
                        {
                            App.Overlay.SetCrosshairState(true, _cursorX, _cursorY);
                            _crosshairShowing = true;
                        } // Otherwise hide crosshair
                        else if (!WoWReader.MouselookState && _crosshairShowing)
                        {
                            App.Overlay.SetCrosshairState(false);
                            _crosshairShowing = false;
                        }
                    }


                    // Check if mouselook is inactive
                    if (!WoWReader.MouselookState)
                    {
                        // Update last known cursor position
                        var cursor = Cursor.Position;
                        _cursorX = (int)cursor.X;
                        _cursorY = (int)cursor.Y;

                        // Check if we need to re-center the mouse cursor
                        if (AppSettings.Default.MemoryAutoCenter &&
                            foregroundWindow == ProcessManager.GameProcess?.MainWindowHandle &&
                            _mouselookStarted != DateTime.MinValue &&
                            DateTime.Now >=
                            _mouselookStarted + TimeSpan.FromMilliseconds(AppSettings.Default.MemoryAutoCenterDelay))
                        {
                            var windowRect = ProcessManager.GetClientRectangle();
                            Cursor.Position = new Point(
                                windowRect.X + windowRect.Width/2,
                                windowRect.Y + windowRect.Height/2);
                        }

                        // Reset auto-center cooldown timer
                        _mouselookStarted = DateTime.MinValue;
                    }

                    // Check if mouselook is active
                    if (WoWReader.MouselookState)
                    {
                        // If so, start the cooldown timer
                        if (_mouselookStarted == DateTime.MinValue)
                            _mouselookStarted = DateTime.Now;

                        // If the timer has elapsed but mouselook is active, temporarily hide the crosshair
                        else if (AppSettings.Default.EnableOverlayCrosshair &&
                                 AppSettings.Default.MemoryAutoCenter &&
                                 DateTime.Now >= _mouselookStarted +
                                 TimeSpan.FromMilliseconds(AppSettings.Default.MemoryAutoCenterDelay) && _crosshairShowing &&
                                 App.Overlay.CrosshairVisible)
                        {
                            App.Overlay.SetCrosshairState(false);
                        }
                    }
                }

                Thread.Sleep(5);
            }
        }

        private static void ProcessMovement(Point axis)
        {
            var sendLeft = -axis.X > AppSettings.Default.MovementThreshold;
            var sendRight = axis.X > AppSettings.Default.MovementThreshold;
            var sendUp = -axis.Y > AppSettings.Default.MovementThreshold;
            var sendDown = axis.Y > AppSettings.Default.MovementThreshold;

            var strength = Math.Sqrt(axis.X*axis.X + axis.Y*axis.Y);
            if (AppSettings.Default.MemoryAutoWalk &&  // AUTO WALK DISABLED
                WoWReader.IsAttached && 
                WoWReader.GameState)
            {
                var moveState = WoWReader.MovementState;
                if (moveState == 0 || moveState == 1)
                {
                    if (strength < AppSettings.Default.WalkThreshold &&
                        strength >= AppSettings.Default.MovementThreshold &&
                        moveState == 0) // Activate Walk
                    {
                        WoWInput.SendKeyDown(Key.Divide);
                        WoWInput.SendKeyUp(Key.Divide);
                        _stopWalk = false;
                    }
                    else if (strength >= AppSettings.Default.WalkThreshold && moveState == 1) // Deactivate walk, start run
                    {
                        WoWInput.SendKeyDown(Key.Divide);
                        WoWInput.SendKeyUp(Key.Divide);
                    }
                    else if (strength < AppSettings.Default.MovementThreshold && !_stopWalk && moveState == 1)
                        // Deactivate walk, stop moving
                    {
                        WoWInput.SendKeyDown(Key.Divide);
                        WoWInput.SendKeyUp(Key.Divide);
                        _stopWalk = true;
                    }
                }
            }
            if (sendLeft)
            {
                if (!_keyStates[(int) GamepadButton.LeftStickLeft])
                {
                    WoWInput.SendKeyDown(BindManager.GetKey(GamepadButton.LeftStickLeft));
                    _keyStates[(int) GamepadButton.LeftStickLeft] = true;
                }
            }
            else
            {
                if (_keyStates[(int) GamepadButton.LeftStickLeft])
                {
                    WoWInput.SendKeyUp(BindManager.GetKey(GamepadButton.LeftStickLeft));
                    _keyStates[(int) GamepadButton.LeftStickLeft] = false;
                }
            }

            if (sendRight)
            {
                if (!_keyStates[(int) GamepadButton.LeftStickRight])
                {
                    WoWInput.SendKeyDown(BindManager.GetKey(GamepadButton.LeftStickRight));
                    _keyStates[(int) GamepadButton.LeftStickRight] = true;
                }
            }
            else
            {
                if (_keyStates[(int) GamepadButton.LeftStickRight])
                {
                    WoWInput.SendKeyUp(BindManager.GetKey(GamepadButton.LeftStickRight));
                    _keyStates[(int) GamepadButton.LeftStickRight] = false;
                }
            }

            if (sendUp)
            {
                if (!_keyStates[(int) GamepadButton.LeftStickUp])
                {
                    WoWInput.SendKeyDown(BindManager.GetKey(GamepadButton.LeftStickUp));
                    _keyStates[(int) GamepadButton.LeftStickUp] = true;
                }
            }
            else
            {
                if (_keyStates[(int) GamepadButton.LeftStickUp])
                {
                    WoWInput.SendKeyUp(BindManager.GetKey(GamepadButton.LeftStickUp));
                    _keyStates[(int) GamepadButton.LeftStickUp] = false;
                }
            }

            if (sendDown)
            {
                if (!_keyStates[(int) GamepadButton.LeftStickDown])
                {
                    WoWInput.SendKeyDown(BindManager.GetKey(GamepadButton.LeftStickDown));
                    _keyStates[(int) GamepadButton.LeftStickDown] = true;
                }
            }
            else
            {
                if (_keyStates[(int) GamepadButton.LeftStickDown])
                {
                    WoWInput.SendKeyUp(BindManager.GetKey(GamepadButton.LeftStickDown));
                    _keyStates[(int) GamepadButton.LeftStickDown] = false;
                }
            }
        }

        private static void ProcessCursor(Point axis)
        {
            var axisInput = new Vector2(axis.X, axis.Y);
            if (axisInput.Magnitude > AppSettings.Default.CursorDeadzone)
            {
                axisInput = Vector2.Normalize(axisInput)*
                            ((axisInput.Magnitude - AppSettings.Default.CursorDeadzone)/
                             (127 - AppSettings.Default.CursorDeadzone));

                var curve = 0.05*AppSettings.Default.CursorCurve;

                var xSpeed = (axisInput.X < 0 ? -axisInput.X : axisInput.X)*AppSettings.Default.CursorSpeed;
                var ySpeed = (axisInput.Y < 0 ? -axisInput.Y : axisInput.Y)*AppSettings.Default.CursorSpeed;

                var xMath = Math.Pow(curve*xSpeed, 2) + curve*xSpeed;
                var yMath = Math.Pow(curve*ySpeed, 2) + curve*ySpeed;

                //xMath *= MemoryManager.ReadAoeState() ? 0.5 : 1;
                //yMath *= MemoryManager.ReadAoeState() ? 0.5 : 1;

                var mouseMovement = new Vector2(xMath, yMath);

                if (axis.X < 0) mouseMovement.X = -mouseMovement.X;
                if (axis.Y < 0) mouseMovement.Y = -mouseMovement.Y;

                if (AppSettings.Default.MemoryInvertTurn && WoWReader.MouselookState)
                {
                    // Invert left/right while mouselooking (if enabled)
                    mouseMovement.X *= -1;
                }

                if (AppSettings.Default.InputHardwareMouse)
                { 
                    HardwareInput.MoveMouse((int)mouseMovement.X, (int)mouseMovement.Y);
                }
                else
                { 
                    var m = new Point(Cursor.Position.X + mouseMovement.X, Cursor.Position.Y + mouseMovement.Y);
                    Cursor.Position = m; 
                }
            }
        }

        private static void ProcessCharacterMenu(GamepadButton button, bool state)
        {
            switch (button)
            {
                case GamepadButton.LFaceUp:
                    if (state && !_keyStates[(int) button])
                        WoWInput.SendKeyDown(Key.Up);
                    if (!state && _keyStates[(int) button])
                        WoWInput.SendKeyUp(Key.Up);
                    break;
                case GamepadButton.LFaceDown:
                    if (state && !_keyStates[(int) button])
                        WoWInput.SendKeyDown(Key.Down);
                    if (!state && _keyStates[(int) button])
                        WoWInput.SendKeyUp(Key.Down);
                    break;
                case GamepadButton.RFaceDown:
                    if (state && !_keyStates[(int) button])
                        WoWInput.SendKeyDown(Key.Enter);
                    if (!state && _keyStates[(int) button])
                        WoWInput.SendKeyUp(Key.Enter);
                    break;
                case GamepadButton.CenterMiddle:
                    if (state && !_keyStates[(int) button])
                        WoWInput.SendKeyDown(Key.Escape);
                    if (!state && _keyStates[(int) button])
                        WoWInput.SendKeyUp(Key.Escape);
                    break;
            }

            // Do left/right mouse buttons
            if (button == GamepadButton.LeftStick)
            {
                if (state)
                    WoWInput.SendMouseDown(MouseButton.Left);
                else
                    WoWInput.SendMouseUp(MouseButton.Left);
            }
            else if (button == GamepadButton.RightStick)
            {
                if (state)
                    WoWInput.SendMouseDown(MouseButton.Right);
                else
                    WoWInput.SendMouseUp(MouseButton.Right);
            }

            _keyStates[(int)button] = state;
        }

        private static void ProcessLoginScreen(GamepadButton button, bool state)
        {
            switch (button)
            {
                case GamepadButton.RFaceLeft:
                    if (state && !_keyStates[(int)button])
                        WoWInput.SendKeyDown(Key.Tab);
                    if (!state && _keyStates[(int)button])
                        WoWInput.SendKeyUp(Key.Tab);
                    break;
                case GamepadButton.RFaceRight: 
                    if (state && _keyStates[(int)button])
                        WoWInput.SendKeyUp(Key.Enter); 
                    break;
                case GamepadButton.RFaceUp:
                    if (state && !_keyStates[(int)button])
                        WoWInput.SendKeyDown(Key.Escape);
                    if (!state && _keyStates[(int)button])
                        WoWInput.SendKeyUp(Key.Escape);
                    break; 
                case GamepadButton.ShoulderRight:
                    if (state && !_keyStates[(int)button])
                        WoWInput.SendKeyDown(Key.Back);
                    if (!state && _keyStates[(int)button])
                        WoWInput.SendKeyUp(Key.Back);
                    break;
            }


            // Do left/right mouse buttons
            if (button == GamepadButton.LeftStick)
            {
                if (state)
                    WoWInput.SendMouseDown(MouseButton.Left);
                else
                    WoWInput.SendMouseUp(MouseButton.Left);
            }
            else if (button == GamepadButton.RightStick)
            {
                if (state)
                    WoWInput.SendMouseDown(MouseButton.Right);
                else
                    WoWInput.SendMouseUp(MouseButton.Right);
            }

            _keyStates[(int) button] = state;
        }

        private static void ProcessPlayerAoe(GamepadButton button, bool state)
        {
            if (!_keyStates[(int) button])
            {
                if (button == AppSettings.Default.MemoryAoeConfirm)
                {
                    WoWInput.SendMouseClick(MouseButton.Left);
                    return;
                } 
                if (button == AppSettings.Default.MemoryAoeCancel)
                {
                    WoWInput.SendMouseClick(MouseButton.Right);
                    return;
                }
            }
            ProcessInput(button, state);
        }

        private static void ProcessInput(GamepadButton button, bool state)
        {
            // Do left/right mouse buttons
            if (button == GamepadButton.LeftStick)
            {
                if (state)
                    WoWInput.SendMouseDown(MouseButton.Left);
                else
                    WoWInput.SendMouseUp(MouseButton.Left);
            }
            else if (button == GamepadButton.RightStick)
            {
                if (state)
                    WoWInput.SendMouseDown(MouseButton.Right);
                else
                    WoWInput.SendMouseUp(MouseButton.Right);
            }

            // Do other buttons
            if (_keyStates[(int) button] != state)
            {
                if (state)
                {
                    WoWInput.SendKeyDown(BindManager.GetKey(button));
                }
                else
                {
                    WoWInput.SendKeyUp(BindManager.GetKey(button));
                }
            }

            _keyStates[(int) button] = state;
        }

        private static void ActiveController_ButtonStateChanged(GamepadButton button, bool state)
        {
            if (AppSettings.Default.EnableMemoryReading)
            {
                if (AppSettings.Default.MemoryOverrideLogin && WoWReader.IsAttached && !WoWReader.LoggedState)
                {
                    ProcessLoginScreen(button, state);
                    return; 
                }

                if (AppSettings.Default.MemoryOverrideMenu && WoWReader.IsAttached && !WoWReader.GameState)
                { 
                    ProcessCharacterMenu(button, state);
                    return;
                }
                
                // Process input if player is casting targeted AoE
                if (WoWReader.GameState && AppSettings.Default.MemoryOverrideAoeCast &&
                    WoWReader.AoeState)
                {
                    ProcessPlayerAoe(button, state);
                    return;
                }
            }

            // Process input
            ProcessInput(button, state);
        }

        public static void Start()
        {
            _threadRunning = true;
            _inputThread.Start();
        }

        public static void Stop()
        {
            _threadRunning = false;
            HapticFeedback.Abort();
        }
    }
}