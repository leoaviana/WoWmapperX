using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS4Windows
{
    internal class Global
    {
        public static double Clamp(double min, double value, double max)
        {
            return value < min ? min : value > max ? max : value;
        }

        public static string exelocation = new Func<string>(() =>
        {
            string filePath = Process.GetCurrentProcess().MainModule.FileName;
            DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));
            // Check if exe is placed in a junction symlink directory (done with Scoop).
            // Good enough
            if (dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) &&
                dirInfo.LinkTarget != null)
            {
                // App directory is a junction. Find real directory and get proper path
                // for inserting into HidHide
                filePath = Path.Combine(dirInfo.LinkTarget, Path.GetFileName(filePath));
            }

            return filePath;
        })();

        internal static string GetStringDeviceProperty(string deviceInstanceId,
            NativeMethods.DEVPROPKEY prop)
        {
            string result = string.Empty;
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            ulong propertyType = 0;
            var requiredSize = 0;

            Guid hidGuid = new Guid();
            NativeMethods.HidD_GetHidGuid(ref hidGuid);
            //IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(IntPtr.Zero, deviceInstanceId, 0, extraFlags | NativeMethods.DIGCF_DEVICEINTERFACE | NativeMethods.DIGCF_ALLCLASSES);
            IntPtr deviceInfoSet = NativeMethods.SetupDiCreateDeviceInfoList(IntPtr.Zero, 0);
            //NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            NativeMethods.SetupDiOpenDeviceInfo(deviceInfoSet, deviceInstanceId, IntPtr.Zero, 0, ref deviceInfoData);
            NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref prop, ref propertyType,
                    null, 0, ref requiredSize, 0);

            if (requiredSize > 0)
            {
                byte[] dataBuffer = new byte[requiredSize];
                NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref prop, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0);

                result = dataBuffer.ToUTF16String();
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }


        public static string GetInstanceIdFromDevicePath(string devicePath)
        {
            string result = string.Empty;
            uint requiredSize = 0;
            NativeMethods.CM_Get_Device_Interface_Property(devicePath, ref NativeMethods.DEVPKEY_Device_InstanceId, out _, null, ref requiredSize, 0);
            if (requiredSize > 0)
            {
                byte[] buffer = new byte[requiredSize];
                NativeMethods.CM_Get_Device_Interface_Property(devicePath, ref NativeMethods.DEVPKEY_Device_InstanceId, out _, buffer, ref requiredSize, 0);
                result = buffer.ToUTF16String();
            }

            return result;
        }

        internal static string[] GetStringArrayDeviceProperty(string deviceInstanceId,
            NativeMethods.DEVPROPKEY prop)
        {
            string[] result = null;
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            ulong propertyType = 0;
            var requiredSize = 0;

            IntPtr zero = IntPtr.Zero;
            //IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(zero, deviceInstanceId, 0, extraFlags | NativeMethods.DIGCF_DEVICEINTERFACE | NativeMethods.DIGCF_ALLCLASSES);
            IntPtr deviceInfoSet = NativeMethods.SetupDiCreateDeviceInfoList(IntPtr.Zero, 0);
            //NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            NativeMethods.SetupDiOpenDeviceInfo(deviceInfoSet, deviceInstanceId, IntPtr.Zero, 0, ref deviceInfoData);
            NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref prop, ref propertyType,
                    null, 0, ref requiredSize, 0);

            if (requiredSize > 0)
            {
                byte[] dataBuffer = new byte[requiredSize];
                NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref prop, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0);

                string tempStr = Encoding.Unicode.GetString(dataBuffer);
                string[] hardwareIds = tempStr.TrimEnd(new char[] { '\0', '\0' }).Split('\0');
                result = hardwareIds;
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        public static bool CheckIfVirtualDevice(string devicePath)
        {
            bool result = false;
            bool excludeMatchFound = false;

            var instanceId = GetInstanceIdFromDevicePath(devicePath);
            var testInstanceId = instanceId;
            while (!string.IsNullOrEmpty(testInstanceId))
            {
                var hardwareIds = GetStringArrayDeviceProperty(testInstanceId, NativeMethods.DEVPKEY_Device_HardwareIds);
                if (hardwareIds != null)
                {
                    // hardware IDs of root hubs/controllers that emit supported virtual devices as sources
                    var excludedIds = new[]
                    {
                        @"ROOT\HIDGAMEMAP", // reWASD
                        @"ROOT\VHUSB3HC", // VirtualHere
                    };

                    excludeMatchFound = hardwareIds.Any(id => excludedIds.Contains(id.ToUpper()));
                    if (excludeMatchFound)
                    {
                        break;
                    }
                }

                // Check for potential non-present device as well
                string parentInstanceId = GetStringDeviceProperty(testInstanceId, NativeMethods.DEVPKEY_Device_Parent);

                // Found root enumerator. Use instanceId of device one layer lower in final check
                if (parentInstanceId.Equals(@"HTREE\ROOT\0", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                testInstanceId = parentInstanceId;
            }

            if (!excludeMatchFound &&
                !string.IsNullOrEmpty(testInstanceId) &&
                (testInstanceId.StartsWith(@"ROOT\SYSTEM", StringComparison.OrdinalIgnoreCase)
                || testInstanceId.StartsWith(@"ROOT\USB", StringComparison.OrdinalIgnoreCase)))
            {
                result = true;
            }

            return result;
        }
    }
    internal static class AppLogger
    {
        public static void LogToGui(string message, bool b)
        {
            //stub
        }
    }

    public class ReadLocker : IDisposable
    {
        private ReaderWriterLockSlim _lockerInstance;

        public ReadLocker(ReaderWriterLockSlim lockerInstance)
        {
            _lockerInstance = lockerInstance;
            _lockerInstance.EnterReadLock();
        }

        public void Dispose()
        {
            _lockerInstance.ExitReadLock();
            _lockerInstance = null;
        }
    }

    public class WriteLocker : IDisposable
    {
        private ReaderWriterLockSlim _lockerInstance;
        private bool IsDisposed => _lockerInstance == null;

        public WriteLocker(ReaderWriterLockSlim lockerInstance)
        {
            _lockerInstance = lockerInstance;
            _lockerInstance.EnterWriteLock();
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }

            _lockerInstance.ExitWriteLock();
            _lockerInstance = null;
        }
    }
}
