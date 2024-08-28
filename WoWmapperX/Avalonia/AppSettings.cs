using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using WoWmapperX.Controllers;
using System.Text.Json.Serialization;

namespace WoWmapperX.AvaloniaImpl
{
    public class SettingChangingEventArgs(string settingName, object newValue) : EventArgs
    {
        public string SettingName { get; } = settingName;
        public object NewValue { get; } = newValue;
    }

    public class AppSettings : INotifyPropertyChanged
    {
        #region "Privates"
        private bool _disableDonationButton;
        private bool _runInBackground;
        private bool _autoUpdate;
        private string _settingsVersion;
        private string _appTheme;
        private string _appAccent;
        private bool _exportBindings;
        private int _modifierStyle;
        private bool _customBindings;
        private int _walkThreshold;
        private bool _hideAtStartup;
        private byte _triggerThresholdLeft;
        private byte _triggerThresholdRight;
        private int _cursorDeadzone;
        private int _cursorSpeed;
        private int _cursorCurve;
        private bool _enableMemoryReading;
        private bool _swapSticks;
        private int _movementThreshold;
        private bool _inputDirectKeyboard;
        private bool _inputHardwareMouse;
        private bool _memoryOverrideMenu;
        private bool _memoryOverrideAoeCast;
        private bool _memoryAutoWalk;
        private bool _memoryAutoCenter;
        private bool _memoryAutoCancel;
        private bool _memoryVibrationDamage;
        private bool _memoryLightbar;
        private int _buttonStyle;
        private bool _enableLogging;
        private int _memoryAutoCenterDelay;
        private bool _memoryVibrationHealing;
        private DateTime _bindingsModified;
        private bool _enableOverlay;
        private bool _enableOverlayCrosshair;
        private bool _enableOverlayConnection;
        private bool _enableOverlayBattery;
        private int _notificationH;
        private int _notificationV;
        private bool _enableTouchpad;
        private bool _memoryTouchpadCursorOnly;
        private int _touchpadMode;
        private GamepadButton _memoryAoeConfirm;
        private GamepadButton _memoryAoeCancel;
        private bool _xinputOverride;
        private int _xinputDll;
        private bool _memoryInvertTurn;
        private bool _memoryOverrideLogin;
        private List<String> _gameProcessNames;

        #endregion

        private static readonly Dictionary<string, object> DefaultValues = new()
        {
            { nameof(DisableDonationButton), false },
            { nameof(RunInBackground), true },
            { nameof(AutoUpdate), false },
            { nameof(SettingsVersion), "0.0.0.0" },
            { nameof(AppTheme), "Dark" },
            { nameof(AppAccent), "DeepPurple" },
            { nameof(ExportBindings), true },
            { nameof(ModifierStyle), 0 },
            { nameof(CustomBindings), false },
            { nameof(WalkThreshold), 70 },
            { nameof(HideAtStartup), false },
            { nameof(TriggerThresholdLeft), (byte)80 },
            { nameof(TriggerThresholdRight), (byte)80 },
            { nameof(CursorDeadzone), 20 },
            { nameof(CursorSpeed), 16 },
            { nameof(CursorCurve), 4 },
            { nameof(EnableMemoryReading), false },
            { nameof(SwapSticks), false },
            { nameof(MovementThreshold), 40 },
            { nameof(InputDirectKeyboard), true },
            { nameof(InputHardwareMouse), false },
            { nameof(MemoryOverrideMenu), true },
            { nameof(MemoryOverrideAoeCast), true },
            { nameof(MemoryAutoWalk), true },
            { nameof(MemoryAutoCenter), true },
            { nameof(MemoryAutoCancel), true },
            { nameof(MemoryVibrationDamage), true },
            { nameof(MemoryLightbar), true },
            { nameof(ButtonStyle), 0 },
            { nameof(EnableLogging), true },
            { nameof(MemoryAutoCenterDelay), 1000 },
            { nameof(MemoryVibrationHealing), true },
            { nameof(BindingsModified), new DateTime(1970, 1, 1, 0, 0, 0) },
            { nameof(EnableOverlay), false },
            { nameof(EnableOverlayCrosshair), true },
            { nameof(EnableOverlayConnection), true },
            { nameof(EnableOverlayBattery), true },
            { nameof(NotificationH), 2 },
            { nameof(NotificationV), 2 },
            { nameof(EnableTouchpad), true },
            { nameof(MemoryTouchpadCursorOnly), false },
            { nameof(TouchpadMode), 0 },
            { nameof(MemoryAoeConfirm), GamepadButton.RFaceDown },
            { nameof(MemoryAoeCancel), GamepadButton.RFaceRight },
            { nameof(XinputOverride), false },
            { nameof(XinputDll), 0 },
            { nameof(MemoryInvertTurn), false },
            { nameof(MemoryOverrideLogin), true },
            { nameof(GameProcessNames), (new[] {"wow", "wow-64", "wowt", "wowt-64", "wowb", "wowb-64"}).ToList() }
        };

        #region "Properties"
        public bool DisableDonationButton
        {
            get => _disableDonationButton;
            set
            {
                if (_disableDonationButton != value)
                {
                    OnSettingChanging(nameof(DisableDonationButton), value);
                    _disableDonationButton = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool RunInBackground
        {
            get => _runInBackground;
            set
            {
                if (_runInBackground != value)
                {
                    OnSettingChanging(nameof(RunInBackground), value);
                    _runInBackground = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                if (_autoUpdate != value)
                {
                    OnSettingChanging(nameof(AutoUpdate), value);
                    _autoUpdate = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SettingsVersion
        {
            get => _settingsVersion;
            set
            {
                if (_settingsVersion != value)
                {
                    OnSettingChanging(nameof(SettingsVersion), value);
                    _settingsVersion = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AppTheme
        {
            get => _appTheme;
            set
            {
                if (_appTheme != value)
                {
                    OnSettingChanging(nameof(AppTheme), value);
                    _appTheme = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AppAccent
        {
            get => _appAccent;
            set
            {
                if (_appAccent != value)
                {
                    OnSettingChanging(nameof(AppAccent), value);
                    _appAccent = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool ExportBindings
        {
            get => _exportBindings;
            set
            {
                if (_exportBindings != value)
                {
                    OnSettingChanging(nameof(ExportBindings), value);
                    _exportBindings = value;
                    OnPropertyChanged();
                }
            }
        }
        public int ModifierStyle
        {
            get => _modifierStyle;
            set
            {
                if (_modifierStyle != value)
                {
                    OnSettingChanging(nameof(ModifierStyle), value);
                    _modifierStyle = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool CustomBindings
        {
            get => _customBindings;
            set
            {
                if (_customBindings != value)
                {
                    OnSettingChanging(nameof(CustomBindings), value);
                    _customBindings = value;
                    OnPropertyChanged();
                }
            }
        }
        public int WalkThreshold
        {
            get => _walkThreshold;
            set
            {
                if (_walkThreshold != value)
                {
                    OnSettingChanging(nameof(WalkThreshold), value);
                    _walkThreshold = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool HideAtStartup
        {
            get => _hideAtStartup;
            set
            {
                if (_hideAtStartup != value)
                {
                    OnSettingChanging(nameof(HideAtStartup), value);
                    _hideAtStartup = value;
                    OnPropertyChanged();
                }
            }
        }
        public byte TriggerThresholdLeft
        {
            get => _triggerThresholdLeft;
            set
            {
                if (_triggerThresholdLeft != value)
                {
                    OnSettingChanging(nameof(TriggerThresholdLeft), value);
                    _triggerThresholdLeft = value;
                    OnPropertyChanged();
                }
            }
        }
        public byte TriggerThresholdRight
        {
            get => _triggerThresholdRight;
            set
            {
                if (_triggerThresholdRight != value)
                {
                    OnSettingChanging(nameof(TriggerThresholdRight), value);
                    _triggerThresholdRight = value;
                    OnPropertyChanged();
                }
            }
        }
        public int CursorDeadzone
        {
            get => _cursorDeadzone;
            set
            {
                if (_cursorDeadzone != value)
                {
                    OnSettingChanging(nameof(CursorDeadzone), value);
                    _cursorDeadzone = value;
                    OnPropertyChanged();
                }
            }
        }
        public int CursorSpeed
        {
            get => _cursorSpeed;
            set
            {
                if (_cursorSpeed != value)
                {
                    OnSettingChanging(nameof(CursorSpeed), value);
                    _cursorSpeed = value;
                    OnPropertyChanged();
                }
            }
        }
        public int CursorCurve
        {
            get => _cursorCurve;
            set
            {
                if (_cursorCurve != value)
                {
                    OnSettingChanging(nameof(CursorCurve), value);
                    _cursorCurve = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool EnableMemoryReading
        {
            get => _enableMemoryReading;
            set
            {
                if (_enableMemoryReading != value)
                {
                    OnSettingChanging(nameof(EnableMemoryReading), value);
                    _enableMemoryReading = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool SwapSticks
        {
            get => _swapSticks;
            set
            {
                if (_swapSticks != value)
                {
                    OnSettingChanging(nameof(SwapSticks), value);
                    _swapSticks = value;
                    OnPropertyChanged();
                }
            }
        }
        public int MovementThreshold
        {
            get => _movementThreshold;
            set
            {
                if (_movementThreshold != value)
                {
                    OnSettingChanging(nameof(MovementThreshold), value);
                    _movementThreshold = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool InputDirectKeyboard
        {
            get => _inputDirectKeyboard;
            set
            {
                if (_inputDirectKeyboard != value)
                {
                    OnSettingChanging(nameof(InputDirectKeyboard), value);
                    _inputDirectKeyboard = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool InputHardwareMouse
        {
            get => _inputHardwareMouse;
            set
            {
                if (_inputHardwareMouse != value)
                {
                    OnSettingChanging(nameof(InputHardwareMouse), value);
                    _inputHardwareMouse = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryOverrideMenu
        {
            get => _memoryOverrideMenu;
            set
            {
                if (_memoryOverrideMenu != value)
                {
                    OnSettingChanging(nameof(MemoryOverrideMenu), value);
                    _memoryOverrideMenu = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryOverrideAoeCast
        {
            get => _memoryOverrideAoeCast;
            set
            {
                if (_memoryOverrideAoeCast != value)
                {
                    OnSettingChanging(nameof(MemoryOverrideAoeCast), value);
                    _memoryOverrideAoeCast = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryAutoWalk
        {
            get => _memoryAutoWalk;
            set
            {
                if (_memoryAutoWalk != value)
                {
                    OnSettingChanging(nameof(MemoryAutoWalk), value);
                    _memoryAutoWalk = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryAutoCenter
        {
            get => _memoryAutoCenter;
            set
            {
                if (_memoryAutoCenter != value)
                {
                    OnSettingChanging(nameof(MemoryAutoCenter), value);
                    _memoryAutoCenter = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryAutoCancel
        {
            get => _memoryAutoCancel;
            set
            {
                if (_memoryAutoCancel != value)
                {
                    OnSettingChanging(nameof(MemoryAutoCancel), value);
                    _memoryAutoCancel = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryVibrationDamage
        {
            get => _memoryVibrationDamage;
            set
            {
                if (_memoryVibrationDamage != value)
                {
                    OnSettingChanging(nameof(MemoryVibrationDamage), value);
                    _memoryVibrationDamage = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryLightbar
        {
            get => _memoryLightbar;
            set
            {
                if (_memoryLightbar != value)
                {
                    OnSettingChanging(nameof(MemoryLightbar), value);
                    _memoryLightbar = value;
                    OnPropertyChanged();
                }
            }
        }
        public int ButtonStyle
        {
            get => _buttonStyle;
            set
            {
                if (_buttonStyle != value)
                {
                    OnSettingChanging(nameof(ButtonStyle), value);
                    _buttonStyle = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool EnableLogging
        {
            get => _enableLogging;
            set
            {
                if (_enableLogging != value)
                {
                    OnSettingChanging(nameof(EnableLogging), value);
                    _enableLogging = value;
                    OnPropertyChanged();
                }
            }
        }
        public int MemoryAutoCenterDelay
        {
            get => _memoryAutoCenterDelay;
            set
            {
                if (_memoryAutoCenterDelay != value)
                {
                    OnSettingChanging(nameof(MemoryAutoCenterDelay), value);
                    _memoryAutoCenterDelay = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryVibrationHealing
        {
            get => _memoryVibrationHealing;
            set
            {
                if (_memoryVibrationHealing != value)
                {
                    OnSettingChanging(nameof(MemoryVibrationHealing), value);
                    _memoryVibrationHealing = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime BindingsModified
        {
            get => _bindingsModified;
            set
            {
                if (_bindingsModified != value)
                {
                    OnSettingChanging(nameof(BindingsModified), value);
                    _bindingsModified = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool EnableOverlay
        {
            get => _enableOverlay;
            set
            {
                if (_enableOverlay != value)
                {
                    OnSettingChanging(nameof(EnableOverlay), value);
                    _enableOverlay = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool EnableOverlayCrosshair
        {
            get => _enableOverlayCrosshair;
            set
            {
                if (_enableOverlayCrosshair != value)
                {
                    OnSettingChanging(nameof(EnableOverlayCrosshair), value);
                    _enableOverlayCrosshair = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool EnableOverlayConnection
        {
            get => _enableOverlayConnection;
            set
            {
                if (_enableOverlayConnection != value)
                {
                    OnSettingChanging(nameof(EnableOverlayConnection), value);
                    _enableOverlayConnection = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool EnableOverlayBattery
        {
            get => _enableOverlayBattery;
            set
            {
                if (_enableOverlayBattery != value)
                {
                    OnSettingChanging(nameof(EnableOverlayBattery), value);
                    _enableOverlayBattery = value;
                    OnPropertyChanged();
                }
            }
        }
        public int NotificationH
        {
            get => _notificationH;
            set
            {
                if (_notificationH != value)
                {
                    OnSettingChanging(nameof(NotificationH), value);
                    _notificationH = value;
                    OnPropertyChanged();
                }
            }
        }
        public int NotificationV
        {
            get => _notificationV;
            set
            {
                if (_notificationV != value)
                {
                    OnSettingChanging(nameof(NotificationV), value);
                    _notificationV = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool EnableTouchpad
        {
            get => _enableTouchpad;
            set
            {
                if (_enableTouchpad != value)
                {
                    OnSettingChanging(nameof(EnableTouchpad), value);
                    _enableTouchpad = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryTouchpadCursorOnly
        {
            get => _memoryTouchpadCursorOnly;
            set
            {
                if (_memoryTouchpadCursorOnly != value)
                {
                    OnSettingChanging(nameof(MemoryTouchpadCursorOnly), value);
                    _memoryTouchpadCursorOnly = value;
                    OnPropertyChanged();
                }
            }
        }
        public int TouchpadMode
        {
            get => _touchpadMode;
            set
            {
                if (_touchpadMode != value)
                {
                    OnSettingChanging(nameof(TouchpadMode), value);
                    _touchpadMode = value;
                    OnPropertyChanged();
                }
            }
        }
        public GamepadButton MemoryAoeConfirm
        {
            get => _memoryAoeConfirm;
            set
            {
                if (_memoryAoeConfirm != value)
                {
                    OnSettingChanging(nameof(MemoryAoeConfirm), value);
                    _memoryAoeConfirm = value;
                    OnPropertyChanged();
                }
            }
        }
        public GamepadButton MemoryAoeCancel
        {
            get => _memoryAoeCancel;
            set
            {
                if (_memoryAoeCancel != value)
                {
                    OnSettingChanging(nameof(MemoryAoeCancel), value);
                    _memoryAoeCancel = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool XinputOverride
        {
            get => _xinputOverride;
            set
            {
                if (_xinputOverride != value)
                {
                    OnSettingChanging(nameof(XinputOverride), value);
                    _xinputOverride = value;
                    OnPropertyChanged();
                }
            }
        }
        public int XinputDll
        {
            get => _xinputDll;
            set
            {
                if (_xinputDll != value)
                {
                    OnSettingChanging(nameof(XinputDll), value);
                    _xinputDll = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryInvertTurn
        {
            get => _memoryInvertTurn;
            set
            {
                if (_memoryInvertTurn != value)
                {
                    OnSettingChanging(nameof(MemoryInvertTurn), value);
                    _memoryInvertTurn = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool MemoryOverrideLogin
        {
            get => _memoryOverrideLogin;
            set
            {
                if (_memoryOverrideLogin != value)
                {
                    OnSettingChanging(nameof(MemoryOverrideLogin), value);
                    _memoryOverrideLogin = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<String> GameProcessNames
        {
            get => _gameProcessNames;
            set
            {
                if (_gameProcessNames != value)
                {
                    OnSettingChanging(nameof(GameProcessNames), value);
                    _gameProcessNames = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<SettingChangingEventArgs> SettingChanging;

        private static readonly object _lock = new();
        private static readonly string settingsFilePath = "settings.json";
        private static readonly string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWmapperX", settingsFilePath);

        [JsonIgnore]
        public static AppSettings Default { get; private set; } = Load();

        [JsonConstructor]
        public AppSettings() { Reset(); }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSettingChanging(string settingName, object newValue)
        {
            SettingChanging?.Invoke(this, new SettingChangingEventArgs(settingName, newValue));
        }

        public void Reset()
        {
            DisableDonationButton = (bool)DefaultValues[nameof(DisableDonationButton)];
            RunInBackground = (bool)DefaultValues[nameof(RunInBackground)];
            AutoUpdate = (bool)DefaultValues[nameof(AutoUpdate)];
            SettingsVersion = (string)DefaultValues[nameof(SettingsVersion)];
            AppTheme = (string)DefaultValues[nameof(AppTheme)];
            AppAccent = (string)DefaultValues[nameof(AppAccent)];
            ExportBindings = (bool)DefaultValues[nameof(ExportBindings)];
            ModifierStyle = (int)DefaultValues[nameof(ModifierStyle)];
            CustomBindings = (bool)DefaultValues[nameof(CustomBindings)];
            WalkThreshold = (int)DefaultValues[nameof(WalkThreshold)];
            HideAtStartup = (bool)DefaultValues[nameof(HideAtStartup)];
            TriggerThresholdLeft = (byte)DefaultValues[nameof(TriggerThresholdLeft)];
            TriggerThresholdRight = (byte)DefaultValues[nameof(TriggerThresholdRight)];
            CursorDeadzone = (int)DefaultValues[nameof(CursorDeadzone)];
            CursorSpeed = (int)DefaultValues[nameof(CursorSpeed)];
            CursorCurve = (int)DefaultValues[nameof(CursorCurve)];
            EnableMemoryReading = (bool)DefaultValues[nameof(EnableMemoryReading)];
            SwapSticks = (bool)DefaultValues[nameof(SwapSticks)];
            MovementThreshold = (int)DefaultValues[nameof(MovementThreshold)];
            InputDirectKeyboard = (bool)DefaultValues[nameof(InputDirectKeyboard)];
            InputHardwareMouse = (bool)DefaultValues[nameof(InputHardwareMouse)];
            MemoryOverrideMenu = (bool)DefaultValues[nameof(MemoryOverrideMenu)];
            MemoryOverrideAoeCast = (bool)DefaultValues[nameof(MemoryOverrideAoeCast)];
            MemoryAutoWalk = (bool)DefaultValues[nameof(MemoryAutoWalk)];
            MemoryAutoCenter = (bool)DefaultValues[nameof(MemoryAutoCenter)];
            MemoryAutoCancel = (bool)DefaultValues[nameof(MemoryAutoCancel)];
            MemoryVibrationDamage = (bool)DefaultValues[nameof(MemoryVibrationDamage)];
            MemoryLightbar = (bool)DefaultValues[nameof(MemoryLightbar)];
            ButtonStyle = (int)DefaultValues[nameof(ButtonStyle)];
            EnableLogging = (bool)DefaultValues[nameof(EnableLogging)];
            MemoryAutoCenterDelay = (int)DefaultValues[nameof(MemoryAutoCenterDelay)];
            MemoryVibrationHealing = (bool)DefaultValues[nameof(MemoryVibrationHealing)];
            BindingsModified = (DateTime)DefaultValues[nameof(BindingsModified)];
            EnableOverlay = (bool)DefaultValues[nameof(EnableOverlay)];
            EnableOverlayCrosshair = (bool)DefaultValues[nameof(EnableOverlayCrosshair)];
            EnableOverlayConnection = (bool)DefaultValues[nameof(EnableOverlayConnection)];
            EnableOverlayBattery = (bool)DefaultValues[nameof(EnableOverlayBattery)];
            NotificationH = (int)DefaultValues[nameof(NotificationH)];
            NotificationV = (int)DefaultValues[nameof(NotificationV)];
            EnableTouchpad = (bool)DefaultValues[nameof(EnableTouchpad)];
            MemoryTouchpadCursorOnly = (bool)DefaultValues[nameof(MemoryTouchpadCursorOnly)];
            TouchpadMode = (int)DefaultValues[nameof(TouchpadMode)];
            MemoryAoeConfirm = (GamepadButton)DefaultValues[nameof(MemoryAoeConfirm)];
            MemoryAoeCancel = (GamepadButton)DefaultValues[nameof(MemoryAoeCancel)];
            XinputOverride = (bool)DefaultValues[nameof(XinputOverride)];
            XinputDll = (int)DefaultValues[nameof(XinputDll)];
            MemoryInvertTurn = (bool)DefaultValues[nameof(MemoryInvertTurn)];
            MemoryOverrideLogin = (bool)DefaultValues[nameof(MemoryOverrideLogin)];
            GameProcessNames = (List<String>)DefaultValues[nameof(GameProcessNames)];
        }

        private static string ReadSettingsFile()
        {
            try
            {
                return File.ReadAllText(settingsFilePath);
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

        public static AppSettings Load()
        {
            var settings = new AppSettings();

            if (File.Exists(settingsFilePath))
            {
                try
                {
                    var json = ReadSettingsFile();
                    settings = JsonSerializer.Deserialize<AppSettings>(json, AppSettingsGenerationContext.Default.AppSettings);

                }
                catch (Exception ex)
                {
                    Log.WriteLine($"Error loading settings: {ex.Message}");
                }
            }

            return settings;
        }

        public void Save()
        {
            lock (_lock)
            {
                var settingsToSave = new AppSettings();

                CheckDefault(this, settingsToSave);

                var json = JsonSerializer.Serialize(settingsToSave, AppSettingsGenerationContext.Default.AppSettings);
                try
                {
                    try
                    {
                        File.WriteAllText(settingsFilePath, json);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        File.WriteAllText(userPath, json);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"Error saving settings: {ex.Message}");
                }
            }
        }

        public void CheckDefault(AppSettings source, AppSettings target)
        {
            if(DisableDonationButton != (bool)DefaultValues[nameof(DisableDonationButton)]) { target.DisableDonationButton = source.DisableDonationButton; }
            if(RunInBackground != (bool)DefaultValues[nameof(RunInBackground)]) { target.RunInBackground = source.RunInBackground; }
            if(AutoUpdate != (bool)DefaultValues[nameof(AutoUpdate)]) { target.AutoUpdate = source.AutoUpdate; }
            if(SettingsVersion != (string)DefaultValues[nameof(SettingsVersion)]) { target.SettingsVersion = source.SettingsVersion; }
            if(AppTheme != (string)DefaultValues[nameof(AppTheme)]) { target.AppTheme = source.AppTheme; }
            if(AppAccent != (string)DefaultValues[nameof(AppAccent)]) { target.AppAccent = source.AppAccent; }
            if(ExportBindings == (bool)DefaultValues[nameof(ExportBindings)]) { target.ExportBindings = source.ExportBindings; }
            if(ModifierStyle != (int)DefaultValues[nameof(ModifierStyle)]) { target.ModifierStyle = source.ModifierStyle; }
            if(CustomBindings != (bool)DefaultValues[nameof(CustomBindings)]) { target.CustomBindings = source.CustomBindings; }
            if(WalkThreshold != (int)DefaultValues[nameof(WalkThreshold)]) { target.WalkThreshold = source.WalkThreshold; }
            if(HideAtStartup != (bool)DefaultValues[nameof(HideAtStartup)]) { target.HideAtStartup = source.HideAtStartup; }
            if(TriggerThresholdLeft != (byte)DefaultValues[nameof(TriggerThresholdLeft)]) { target.TriggerThresholdLeft = source.TriggerThresholdLeft; }
            if(TriggerThresholdRight != (byte)DefaultValues[nameof(TriggerThresholdRight)]) { target.TriggerThresholdRight = source.TriggerThresholdRight; }
            if(CursorDeadzone != (int)DefaultValues[nameof(CursorDeadzone)]) { target.CursorDeadzone = source.CursorDeadzone; }
            if(CursorSpeed != (int)DefaultValues[nameof(CursorSpeed)]) { target.CursorSpeed = source.CursorSpeed; }
            if(CursorCurve != (int)DefaultValues[nameof(CursorCurve)]) { target.CursorCurve = source.CursorCurve; }
            if(EnableMemoryReading != (bool)DefaultValues[nameof(EnableMemoryReading)]) { target.EnableMemoryReading = source.EnableMemoryReading; }
            if(SwapSticks != (bool)DefaultValues[nameof(SwapSticks)]) { target.SwapSticks = source.SwapSticks; }
            if(MovementThreshold != (int)DefaultValues[nameof(MovementThreshold)]) { target.MovementThreshold = source.MovementThreshold; }
            if(InputDirectKeyboard != (bool)DefaultValues[nameof(InputDirectKeyboard)]) { target.InputDirectKeyboard = source.InputDirectKeyboard; }
            if(InputHardwareMouse != (bool)DefaultValues[nameof(InputHardwareMouse)]) { target.InputHardwareMouse = source.InputHardwareMouse; }
            if(MemoryOverrideMenu != (bool)DefaultValues[nameof(MemoryOverrideMenu)]) { target.MemoryOverrideMenu = source.MemoryOverrideMenu; }
            if(MemoryOverrideAoeCast != (bool)DefaultValues[nameof(MemoryOverrideAoeCast)]) { target.MemoryOverrideAoeCast = source.MemoryOverrideAoeCast; }
            if(MemoryAutoWalk != (bool)DefaultValues[nameof(MemoryAutoWalk)]) { target.MemoryAutoWalk = source.MemoryAutoWalk; }
            if(MemoryAutoCenter != (bool)DefaultValues[nameof(MemoryAutoCenter)]) { target.MemoryAutoCenter = source.MemoryAutoCenter; }
            if(MemoryAutoCancel != (bool)DefaultValues[nameof(MemoryAutoCancel)]) { target.MemoryAutoCancel = source.MemoryAutoCancel; }
            if(MemoryVibrationDamage != (bool)DefaultValues[nameof(MemoryVibrationDamage)]) { target.MemoryVibrationDamage = source.MemoryVibrationDamage; }
            if(MemoryLightbar != (bool)DefaultValues[nameof(MemoryLightbar)]) { target.MemoryLightbar = source.MemoryLightbar; }
            if(ButtonStyle != (int)DefaultValues[nameof(ButtonStyle)]) { target.ButtonStyle = source.ButtonStyle; }
            if(EnableLogging != (bool)DefaultValues[nameof(EnableLogging)]) { target.EnableLogging = source.EnableLogging; }
            if(MemoryAutoCenterDelay != (int)DefaultValues[nameof(MemoryAutoCenterDelay)]) { target.MemoryAutoCenterDelay = source.MemoryAutoCenterDelay; }
            if(MemoryVibrationHealing != (bool)DefaultValues[nameof(MemoryVibrationHealing)]) { target.MemoryVibrationHealing = source.MemoryVibrationHealing; }
            if(BindingsModified != (DateTime)DefaultValues[nameof(BindingsModified)]) { target.BindingsModified = source.BindingsModified; }
            if(EnableOverlay != (bool)DefaultValues[nameof(EnableOverlay)]) { target.EnableOverlay = source.EnableOverlay; }
            if(EnableOverlayCrosshair != (bool)DefaultValues[nameof(EnableOverlayCrosshair)]) { target.EnableOverlayCrosshair = source.EnableOverlayCrosshair; }
            if(EnableOverlayConnection != (bool)DefaultValues[nameof(EnableOverlayConnection)]) { target.EnableOverlayConnection = source.EnableOverlayConnection; }
            if(EnableOverlayBattery != (bool)DefaultValues[nameof(EnableOverlayBattery)]) { target.EnableOverlayBattery = source.EnableOverlayBattery; }
            if(NotificationH != (int)DefaultValues[nameof(NotificationH)]) { target.NotificationH = source.NotificationH; }
            if(NotificationV != (int)DefaultValues[nameof(NotificationV)]) { target.NotificationV = source.NotificationV; }
            if(EnableTouchpad != (bool)DefaultValues[nameof(EnableTouchpad)]) { target.EnableTouchpad = source.EnableTouchpad; }
            if(MemoryTouchpadCursorOnly != (bool)DefaultValues[nameof(MemoryTouchpadCursorOnly)]) { target.MemoryTouchpadCursorOnly = source.MemoryTouchpadCursorOnly; }
            if(TouchpadMode != (int)DefaultValues[nameof(TouchpadMode)]) { target.TouchpadMode = source.TouchpadMode; }
            if(MemoryAoeConfirm != (GamepadButton)DefaultValues[nameof(MemoryAoeConfirm)]) { target.MemoryAoeConfirm = source.MemoryAoeConfirm; }
            if(MemoryAoeCancel != (GamepadButton)DefaultValues[nameof(MemoryAoeCancel)]) { target.MemoryAoeCancel = source.MemoryAoeCancel; }
            if(XinputOverride != (bool)DefaultValues[nameof(XinputOverride)]) { target.XinputOverride = source.XinputOverride; }
            if(XinputDll != (int)DefaultValues[nameof(XinputDll)]) { target.XinputDll = source.XinputDll; }
            if(MemoryInvertTurn != (bool)DefaultValues[nameof(MemoryInvertTurn)]) { target.MemoryInvertTurn = source.MemoryInvertTurn; }
            if(MemoryOverrideLogin != (bool)DefaultValues[nameof(MemoryOverrideLogin)]) { target.MemoryOverrideLogin = source.MemoryOverrideLogin; }
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonSerializable(typeof(AppSettings), GenerationMode = JsonSourceGenerationMode.Metadata)]
    internal partial class AppSettingsGenerationContext : JsonSerializerContext
    {
    }
}
