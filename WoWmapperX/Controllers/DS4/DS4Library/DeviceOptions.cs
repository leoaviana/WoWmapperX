using System; 
using System.Xml; 
using DS4Windows.InputDevices; 

namespace DS4Windows
{
    public class DeviceOptions
    {
        private JoyConDeviceOptions joyConDeviceOpts = new JoyConDeviceOptions();
        public JoyConDeviceOptions JoyConDeviceOpts { get => joyConDeviceOpts; }

        private bool verboseLogMessages;
        public bool VerboseLogMessages { get => verboseLogMessages; set => verboseLogMessages = value; }

        public DeviceOptions()
        {
            // If enabled then DS4Windows shows additional log messages when a gamepad is connected (may be useful to diagnose connection problems).
            // This option is not persistent (ie. not saved into config files), so if enabled then it is reset back to FALSE when DS4Windows is restarted.
            verboseLogMessages = false;
        }
    }

    public abstract class ControllerOptionsStore
    {
        protected InputDeviceType deviceType;
        public InputDeviceType DeviceType { get => deviceType; }

        public ControllerOptionsStore(InputDeviceType deviceType)
        {
            this.deviceType = deviceType;
        }
    }

    public class DS4ControllerOptions : ControllerOptionsStore
    {
        public const string XML_ELEMENT_NAME = "DS4SupportSettings";
        private bool copyCatController;
        public bool IsCopyCat
        {
            get => copyCatController;
            set
            {
                if (copyCatController == value) return;
                copyCatController = value;
                IsCopyCatChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler IsCopyCatChanged;

        public DS4ControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }
    }

    public class DualSenseControllerOptions : ControllerOptionsStore
    {
        public const string XML_ELEMENT_NAME = "DualSenseSupportSettings";

        public enum LEDBarMode : ushort
        {
            Off,
            MultipleControllers,
            BatteryPercentage,
            On,
        }

        public enum MuteLEDMode : ushort
        {
            Off,
            On,
            Pulse
        }

        private LEDBarMode ledMode = LEDBarMode.MultipleControllers;
        public LEDBarMode LedMode
        {
            get => ledMode;
            set
            {
                if (ledMode == value) return;
                ledMode = value;
                LedModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler LedModeChanged;

        private MuteLEDMode muteLedMode = MuteLEDMode.Off;
        public MuteLEDMode MuteLedMode
        {
            get => muteLedMode;
            set
            {
                if (muteLedMode == value) return;
                muteLedMode = value;
                MuteLedModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MuteLedModeChanged;

        public DualSenseControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }
    }

    public class SwitchProControllerOptions : ControllerOptionsStore
    {
        public const string XML_ELEMENT_NAME = "SwitchProSupportSettings";

        private bool enableHomeLED = true;
        public bool EnableHomeLED
        {
            get => enableHomeLED;
            set
            {
                if (enableHomeLED == value) return;
                enableHomeLED = value;
                EnableHomeLEDChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnableHomeLEDChanged;

        public SwitchProControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }
    }

    public class JoyConDeviceOptions
    {
        public const bool DEFAULT_ENABLE = false;
        private bool enabled = DEFAULT_ENABLE;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnabledChanged;

        public enum LinkMode : ushort
        {
            Split,
            Joined,
        }

        public const LinkMode LINK_MODE_DEFAULT = LinkMode.Joined;
        private LinkMode linkedMode = LINK_MODE_DEFAULT;
        public LinkMode LinkedMode
        {
            get => linkedMode;
            set
            {
                if (linkedMode == value) return;
                linkedMode = value;
            }
        }

        public enum JoinedGyroProvider : ushort
        {
            JoyConL,
            JoyConR,
        }

        private JoinedGyroProvider joinGyroProv = JoinedGyroProvider.JoyConR;
        public JoinedGyroProvider JoinGyroProv
        {
            get => joinGyroProv;
            set
            {
                if (joinGyroProv == value) return;
                joinGyroProv = value;
            }
        }
    }

    public class JoyConControllerOptions : ControllerOptionsStore
    {
        public const string XML_ELEMENT_NAME = "JoyConSupportSettings";

        private bool enableHomeLED = true;
        public bool EnableHomeLED
        {
            get => enableHomeLED;
            set
            {
                if (enableHomeLED == value) return;
                enableHomeLED = value;
                EnableHomeLEDChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnableHomeLEDChanged;

        public JoyConControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }
    }
}
