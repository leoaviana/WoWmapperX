﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static DS4Windows.NativeMethods;

namespace DS4Windows
{
    public class HidDevices
    {
        private const int HID_USAGE_JOYSTICK = 0x04;
        private const int HID_USAGE_GAMEPAD = 0x05;
        private static Guid _hidClassGuid = Guid.Empty;

        public static bool IsConnected(string devicePath)
        {
            return EnumerateDevices().Any(x => x.Path == devicePath);
        }

        public static HidDevice GetDevice(string devicePath)
        {
            return Enumerate(devicePath).FirstOrDefault();
        }

        public static IEnumerable<HidDevice> Enumerate()
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description));
        }

        public static IEnumerable<HidDevice> Enumerate(string devicePath)
        {
            return EnumerateDevices().Where(x => x.Path == devicePath).Select(x => new HidDevice(x.Path, x.Description));
        }

        public static IEnumerable<HidDevice> Enumerate(int vendorId, params int[] productIds)
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description)).Where(x => x.Attributes.VendorId == vendorId &&
                                                                                  productIds.Contains(x.Attributes.ProductId));
        }

        public static IEnumerable<HidDevice> Enumerate(int[] vendorIds, params int[] productIds)
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description)).Where(x => vendorIds.Contains(x.Attributes.VendorId) &&
                                                                                  productIds.Contains(x.Attributes.ProductId));
        }

        public static IEnumerable<HidDevice> EnumerateDS4(VidPidInfo[] devInfo)
        {
            int iEnumeratedDevCount = 0;
            List<HidDevice> foundDevs = new List<HidDevice>();
            int devInfoLen = devInfo.Length;
            IEnumerable<DeviceInfo> temp = EnumerateDevices();
            for (var devEnum = temp.GetEnumerator(); devEnum.MoveNext();)
            //for (int i = 0, len = temp.Count(); i < len; i++)
            {
                DeviceInfo x = devEnum.Current;
                //DeviceInfo x = temp.ElementAt(i);               
                HidDevice tempDev = new HidDevice(x.Path, x.Description, x.Parent);
                iEnumeratedDevCount++;
                bool found = false;
                for (int j = 0; !found && j < devInfoLen; j++)
                {
                    VidPidInfo tempInfo = devInfo[j];
                    if ((tempDev.Capabilities.Usage == HID_USAGE_GAMEPAD ||
                        tempDev.Capabilities.Usage == HID_USAGE_JOYSTICK ||
                        tempInfo.featureSet.HasFlag(VidPidFeatureSet.VendorDefinedDevice)) &&
                        tempDev.Attributes.VendorId == tempInfo.vid &&
                        tempDev.Attributes.ProductId == tempInfo.pid)
                    {
                        found = true;
                        foundDevs.Add(tempDev);
                    }
                }
                /*
                if (Global.DeviceOptions.VerboseLogMessages)
                {
                    if (found)
                    {
                        AppLogger.LogToGui($"HID#{iEnumeratedDevCount} CONNECTING to {x.Description}  VID={tempDev.Attributes.VendorHexId}  PID={tempDev.Attributes.ProductHexId}  Usage=0x{tempDev.Capabilities.Usage.ToString("X")}  Version=0x{tempDev.Attributes.Version.ToString("X")}  Path={x.Path}", false);
                    }
                    else
                    {
                        AppLogger.LogToGui($"HID#{iEnumeratedDevCount} Unknown device {x.Description}  VID={tempDev.Attributes.VendorHexId}  PID={tempDev.Attributes.ProductHexId}  Usage=0x{tempDev.Capabilities.Usage.ToString("X")}  Version=0x{tempDev.Attributes.Version.ToString("X")}  Path={x.Path}", false);
                    }
                }
                */
            }

            /*
            if (Global.DeviceOptions.VerboseLogMessages && iEnumeratedDevCount > 0)
            {
                // This EnumerateDS4 method is called 3-4 times when a gamepad is connected. Print out "separator" log msg line between different enumeration loops to make the logfile easier to read
                AppLogger.LogToGui($"-------------------------", false);
            }
            */

            return foundDevs;
        }

        public static IEnumerable<HidDevice> Enumerate(int vendorId)
        {
            return EnumerateDevices().Select(x => new HidDevice(x.Path, x.Description)).Where(x => x.Attributes.VendorId == vendorId);
        }

        private class DeviceInfo
        {
            public string Path { get; set; }
            public string Description { get; set; }
            public string Parent { get; set; }
        }

        private static IEnumerable<DeviceInfo> EnumerateDevices()
        {
            var devices = new List<DeviceInfo>();
            var hidClass = HidClassGuid;
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidClass, null, 0, NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                var deviceInfoData = CreateDeviceInfoData();
                var deviceIndex = 0;

                while (NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, deviceIndex, ref deviceInfoData))
                {
                    deviceIndex += 1;

                    var deviceInterfaceData = new NativeMethods.SP_DEVICE_INTERFACE_DATA();
                    deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
                    var deviceInterfaceIndex = 0;

                    while (NativeMethods.SetupDiEnumDeviceInterfaces(deviceInfoSet, ref deviceInfoData, ref hidClass, deviceInterfaceIndex, ref deviceInterfaceData))
                    {
                        deviceInterfaceIndex++;
                        var devicePath = GetDevicePath(deviceInfoSet, deviceInterfaceData);
                        var description = GetBusReportedDeviceDescription(deviceInfoSet, ref deviceInfoData) ??
                                          GetDeviceDescription(deviceInfoSet, ref deviceInfoData);
                        var parent = GetDeviceParent(deviceInfoSet, ref deviceInfoData);
                        devices.Add(new DeviceInfo { Path = devicePath, Description = description, Parent = parent });
                    }
                }
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
            return devices;
        }

        private static NativeMethods.SP_DEVINFO_DATA CreateDeviceInfoData()
        {
            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();

            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            deviceInfoData.DevInst = 0;
            deviceInfoData.ClassGuid = Guid.Empty;
            deviceInfoData.Reserved = IntPtr.Zero;

            return deviceInfoData;
        }
        private static string FixedCharToString(SP_DEVICE_INTERFACE_DETAIL_DATA detailData)
        {
            unsafe
            {
                char* devicePathPtr = detailData.DevicePath;
                {
                    // Find the null terminator to determine the length of the string
                    int length = 0;
                    while (length < 256 && devicePathPtr[length] != '\0')
                    {
                        length++;
                    }

                    // Create a string from the char pointer
                    return new string(devicePathPtr, 0, length);
                }
            }
        }


        private static  string GetDevicePath(IntPtr deviceInfoSet, NativeMethods.SP_DEVICE_INTERFACE_DATA deviceInterfaceData)
        {
            var bufferSize = 0;
            var interfaceDetail = new NativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA { Size = IntPtr.Size == 4 ? 4 + Marshal.SystemDefaultCharSize : 8 };

            NativeMethods.SetupDiGetDeviceInterfaceDetailBuffer(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0, ref bufferSize, IntPtr.Zero);

            return NativeMethods.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, ref interfaceDetail, bufferSize, ref bufferSize, IntPtr.Zero) ?
                FixedCharToString(interfaceDetail) : null;
        }

        public static Guid HidClassGuid
        {
            get
            {
                if (_hidClassGuid.Equals(Guid.Empty)) NativeMethods.HidD_GetHidGuid(ref _hidClassGuid);
                return _hidClassGuid;
            }
        }

        private static string GetDeviceDescription(IntPtr deviceInfoSet, ref NativeMethods.SP_DEVINFO_DATA devinfoData)
        {
            var descriptionBuffer = new byte[1024];

            var requiredSize = 0;
            var type = 0;

            NativeMethods.SetupDiGetDeviceRegistryProperty(deviceInfoSet,
                                                            ref devinfoData,
                                                            NativeMethods.SPDRP_DEVICEDESC,
                                                            ref type,
                                                            descriptionBuffer,
                                                            descriptionBuffer.Length,
                                                            ref requiredSize);

            return descriptionBuffer.ToUTF8String();
        }

        private static string GetBusReportedDeviceDescription(IntPtr deviceInfoSet, ref NativeMethods.SP_DEVINFO_DATA devinfoData)
        {
            var descriptionBuffer = new byte[1024];

            if (Environment.OSVersion.Version.Major > 5)
            {
                ulong propertyType = 0;
                var requiredSize = 0;

                var _continue = NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet,
                                                                        ref devinfoData,
                                                                        ref NativeMethods.DEVPKEY_Device_BusReportedDeviceDesc,
                                                                        ref propertyType,
                                                                        descriptionBuffer,
                                                                        descriptionBuffer.Length,
                                                                        ref requiredSize,
                                                                        0);

                if (_continue) return descriptionBuffer.ToUTF16String();
            }
            return null;
        }

        private static string GetDeviceParent(IntPtr deviceInfoSet, ref NativeMethods.SP_DEVINFO_DATA devinfoData)
        {
            string result = string.Empty;

            var requiredSize = 0;
            ulong propertyType = 0;

            NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref devinfoData,
                                                        ref NativeMethods.DEVPKEY_Device_Parent, ref propertyType,
                                                        null, 0,
                                                        ref requiredSize, 0);

            if (requiredSize > 0)
            {
                var descriptionBuffer = new byte[requiredSize];
                NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref devinfoData,
                                                        ref NativeMethods.DEVPKEY_Device_Parent, ref propertyType,
                                                        descriptionBuffer, descriptionBuffer.Length,
                                                        ref requiredSize, 0);

                string tmp = System.Text.Encoding.Unicode.GetString(descriptionBuffer);
                if (tmp.EndsWith("\0"))
                {
                    tmp = tmp.Remove(tmp.Length - 1);
                }
                result = tmp;
            }

            return result;
        }
    }
}
