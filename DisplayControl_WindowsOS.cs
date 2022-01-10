#if UNITY_STANDALONE_WIN
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FortySevenE.DisplayManager
{
    public class DisplayDeviceModel_WindowsOS
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsPrimaryDisplay { get; private set; }
        public int DisplayNumber { get; private set; }
        public string MonitorName { get; private set; }
        public string MonitorID { get; private set; }
        public string MonitorKey { get; private set; }
        public string DeviceName { get; private set; }

        public DisplayDeviceModel_WindowsOS(int x, int y, int width, int height, bool primaryDisplay, int displayNumber, string monitorName, string monitorID, string monitorKey, string deviceName)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsPrimaryDisplay = primaryDisplay;
            DisplayNumber = displayNumber;
            MonitorName = monitorName;
            MonitorID = monitorID;
            MonitorKey = monitorKey;
            DeviceName = deviceName;
        }

        public override string ToString()
        {
            return string.Format("{0}: ({1}, {2}) [{3}, {4}] / {5} {6}{7}", DisplayNumber, X, Y, Width, Height, MonitorName, DeviceName, IsPrimaryDisplay ? " (Primary)" : "");
        }
    }

    public class DisplayControl_WindowsOS : MonoBehaviour, IDisplayControl
    {
        #region dll Imports

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static private extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        private static extern bool SetWindowText(IntPtr hwnd, string lpString);

        [DllImport("user32.dll")]
        private static extern long SetWindowLong (IntPtr hWnd, long nIndex, long dwNewLong);

        [DllImport("user32.dll")]
        private static extern long GetWindowLong(IntPtr hwnd, long nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hmon, ref MONITORINFOEX monitorinfo);

        [DllImport("user32.dll")]
        private static extern int SetMenu(IntPtr hWnd, IntPtr hMenu);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RectNative lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, long cch);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RectNative lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        public static string GetWindowClass(IntPtr hWnd)
        {
            var sb = new System.Text.StringBuilder(4096);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            var sb = new System.Text.StringBuilder(4096);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        private static readonly long GWL_STYLE = -16L;
        private static readonly int SW_SHOWMINIMIZED = 2;
        private static readonly int SW_SHOW = 5;

        // SetWindoePos Flag
        //private static readonly int SWP_ASYNCWINDOWPOS = 0x4000;
        private static readonly int SWP_NOSIZE = 0x0001;
        private static readonly int SWP_NOMOVE = 0x0002;
        private static readonly int SWP_NOZORDER = 0x0004;
        //private static readonly int SWP_FRAMECHANGED = 0x0020;
        //private static readonly int SWP_NOSENDCHANGING = 0x0400;
        private static readonly int SWP_NOCOPYBITS = 0x0100;

        private static readonly long WS_BORDER = 0x00800000L; //window with border
        private static readonly long WS_THICKFRAME = 0x00040000L; //window with re-sizing border
        private static readonly long WS_SYSMENU = 0x00080000L;
        private static readonly long WS_MINIMIZEBOX = 0x00020000L; //window with border
        private static readonly long WS_MAXIMIZEBOX = 0x00010000L; //window with border
        private static readonly long WS_DLGFRAME = 0x00400000L; //window with double border but no title
        private static readonly long WS_POPUP = 0x80000000L;
        private static readonly long WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        private struct RectNative
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public bool IsSet { get { return !(Left == 0 && Right == 0 && Top == 0 && Bottom == 0); } }
            public int Width { get { return Right - Left; } }
            public int Height { get { return Bottom - Top; } }
        }
        #endregion

        private Dictionary<int, IntPtr> _unityDisplayPointers;
        private List<DisplayDeviceModel_WindowsOS> _displayDevices;
        private bool _changingFullScreen;

        public void RefreshDisplayPointersAfterNewDisplayAdded ()
        {
            if (_unityDisplayPointers == null || _unityDisplayPointers.Count == 0)
            {
                _unityDisplayPointers = new Dictionary<int, IntPtr>();
                _unityDisplayPointers.Add(0, GetActiveWindow());
            }

            for (int i = 0; i < UnityEngine.Display.displays.Length; i ++)
            {
                if (UnityEngine.Display.displays[i].active)
                {
                    if (!_unityDisplayPointers.ContainsKey(i))
                    {
                        List<IntPtr> allWindowsOfCurrentProcess = GetAllWindowsOfThisProcess();
                        foreach (IntPtr windowPointer in allWindowsOfCurrentProcess)
                        {
                            if (!_unityDisplayPointers.ContainsValue(windowPointer))
                            {
                                if (_unityDisplayPointers.ContainsKey(i))
                                {
                                    _unityDisplayPointers[i] = windowPointer;
                                }
                                else
                                {
                                    _unityDisplayPointers.Add(i, windowPointer);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<IntPtr> GetAllWindowsOfThisProcess ()
        {
            List<IntPtr> unityWindowPointers = new List<IntPtr>();
            uint threadId = GetCurrentThreadId();
            RectNative windowRect;

            EnumThreadWindows(threadId, (hwnd, lParam) =>
            {
                if(GetWindowRect(hwnd, out windowRect))
                {
                    if (windowRect.IsSet)
                    {
                        unityWindowPointers.Add(hwnd);
                    }
                }
                return true;
            }, IntPtr.Zero);

            return unityWindowPointers;
        }

        public void SetWindowStyle(int index, WindowStyle displayStyle)
        {
            if (index < _unityDisplayPointers.Count)
            {
                long windowStyle = 0;
                switch (displayStyle)
                {
                    case WindowStyle.Borderless:
                        windowStyle = WS_POPUP;
                        windowStyle &= ~(WS_CAPTION | WS_BORDER | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
                        SetWindowStyle(index, windowStyle, show: true);
                        break;
                    case WindowStyle.MenuBarNoResize:
                        windowStyle = WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
                        SetWindowStyle(index, windowStyle, show: true);
                        break;
                    case WindowStyle.FullMenuBar:
                        windowStyle = WS_CAPTION | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
                        SetWindowStyle(index, windowStyle, show: true);
                        break;
                    case WindowStyle.FullMenuBarMinimized:
                        windowStyle = WS_CAPTION | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
                        SetWindowStyle(index, windowStyle, show: false);
                        break;
                }
            }
        }

        private void SetWindowStyle (int index, long windowStyle, bool show = true)
        {
            SetWindowLong(_unityDisplayPointers[index], GWL_STYLE, windowStyle);
            SetMenu(_unityDisplayPointers[index], IntPtr.Zero);
            ShowWindowAsync(_unityDisplayPointers[index], show ? SW_SHOW : SW_SHOWMINIMIZED);
        }

        public void SetPositionAndSize(int index, int left, int top, bool relativeToMonitor, int relativeMonitorIndex, int width, int height)
        {
            if (index < _unityDisplayPointers.Count)
            {
                if (relativeToMonitor)
                {
                    DisplayDeviceModel_WindowsOS targetMonitor = GetDisplayFromUnityDisplayIndex(relativeMonitorIndex);

                    if (targetMonitor != null)
                    {
                        left = left + targetMonitor.X;
                        top = top + targetMonitor.Y;
                    }
                }
                SetPositionAndSizeImpl(index, left, top, width, height);
            }
        }

        public void SetPosition(int index, int left, int top, bool relativeToMonitor, int relativeMonitorIndex)
        {
            if (index < _unityDisplayPointers.Count) {
                if (relativeToMonitor)
                {
                    DisplayDeviceModel_WindowsOS targetMonitor = GetDisplayFromUnityDisplayIndex(relativeMonitorIndex);

                    if (targetMonitor != null)
                    {
                        left = left + targetMonitor.X;
                        top = top + targetMonitor.Y;
                    }
                }
                SetPositionImpl(index, left, top);
            }
        }

        public void SetSize(int index, int width, int height)
        {
            if (index < _unityDisplayPointers.Count)
            {
                SetSizeImpl(index, width, height);
            }
        }

        //------------------------All below are modified from Rancon WindowManager

        private RectNative GetDisplayWindowRect (int index)
        {
            RectNative rectNative = new RectNative();
            if (index < _unityDisplayPointers.Count)
            {
                GetWindowRect(_unityDisplayPointers[index], out rectNative);
            }
            return rectNative;
        }

        private void SetPositionAndSizeImpl(int index, int x, int y, int width, int height)
        {
            RectNative windowRect = GetDisplayWindowRect(index);
			if (windowRect.IsSet)
			{
                SetWindowPos(_unityDisplayPointers[index], 0, x, y, width, height, SWP_NOZORDER);
            }
        }

        private void SetPositionImpl(int index, int left, int top)
        {
            if (index < _unityDisplayPointers.Count)
            {
                RectNative windowRect = GetDisplayWindowRect(index);
                if (windowRect.IsSet)
                {
                    SetWindowPos(_unityDisplayPointers[index], 0, left, top, 0, 0, SWP_NOCOPYBITS | SWP_NOZORDER | SWP_NOSIZE);
                }
            }
        }

        private void SetSizeImpl(int index, int width, int height)
        {
            RectNative windowRect = GetDisplayWindowRect(index);
			if (windowRect.IsSet)
			{
				SetWindowPos(_unityDisplayPointers[index], 0, 0, 0, width, height, SWP_NOCOPYBITS | SWP_NOZORDER | SWP_NOMOVE);
			}
        }

        #region Display Devices Hardware

        public DisplayDeviceModel_WindowsOS GetPrimaryDisplayDevice()
        {
            foreach (var display in GetDisplays())
            {
                if (display.IsPrimaryDisplay)
                {
                    return display;
                }
            }

            return null;
        }

        public DisplayDeviceModel_WindowsOS GetDisplayFromUnityDisplayIndex(int unityDisplayIndex)
        {
            foreach (var display in GetDisplays())
            {
                if (display.DisplayNumber == unityDisplayIndex)
                {
                    return display;
                }
            }

            return null;
        }

        public DisplayDeviceModel_WindowsOS GetDisplayFromMonitorName(string monitorName)
        {
            foreach (var display in GetDisplays())
            {
                if (display.MonitorName == monitorName)
                {
                    return display;
                }
            }

            return null;
        }

        public DisplayDeviceModel_WindowsOS GetDisplayFromMonitorID(string monitorID)
        {
            foreach (var display in GetDisplays())
            {
                if (display.MonitorID == monitorID)
                {
                    return display;
                }
            }

            return null;
        }

        public DisplayDeviceModel_WindowsOS GetDisplayFromMonitorKey(string monitorKey)
        {
            foreach (var display in GetDisplays())
            {
                if (display.MonitorKey == monitorKey)
                {
                    return display;
                }
            }

            return null;
        }

        public List<DisplayDeviceModel_WindowsOS> GetDisplays()
        {
            if (_displayDevices == null)
            {
                var device = new DISPLAY_DEVICE();
                device.cb = Marshal.SizeOf(device);
                Dictionary<string, KeyValuePair<int, string>> deviceNamesForDisplays = new Dictionary<string, KeyValuePair<int, string>>();
                for (uint id = 0; EnumDisplayDevices(null, id, ref device, 0); id++)
                {
                    if ((device.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) != 0)
                    {
                        deviceNamesForDisplays.Add(device.DeviceName, new KeyValuePair<int, string>( (int) id, device.DeviceString));
                    }
                }

                _displayDevices = new List<DisplayDeviceModel_WindowsOS>();

                int monitorControlPanelIndex = 0;
                int primaryMonitorCPIndex = int.MaxValue;
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                    delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref RectNative lprcMonitor, IntPtr dwData)
                    {
                        MONITORINFOEX monitor = new MONITORINFOEX();
                        monitor.Size = (uint)Marshal.SizeOf(monitor);

                        bool result = GetMonitorInfo(hMonitor, ref monitor);
                        if (result)
                        {
                            string graphicsDeviceName = "";
                            int monitorId = 0;
                            if (deviceNamesForDisplays.ContainsKey(monitor.DeviceName))
                            {
                                monitorId = deviceNamesForDisplays[monitor.DeviceName].Key;
                                graphicsDeviceName = deviceNamesForDisplays[monitor.DeviceName].Value;
                            }

                            DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
                            displayDevice.cb = Marshal.SizeOf(displayDevice);
                            EnumDisplayDevices(monitor.DeviceName, 0, ref displayDevice, 0);

                            int unityDisplayIndex = monitorControlPanelIndex;
                            if (monitor.Flags == 1)
                            {
                                primaryMonitorCPIndex = monitorControlPanelIndex;
                                unityDisplayIndex = 0;
                            }
                            else
                            {
                                if (monitorControlPanelIndex < primaryMonitorCPIndex)
                                {
                                    unityDisplayIndex = unityDisplayIndex + 1;
                                }
                            }

                            DisplayDeviceModel_WindowsOS display = new DisplayDeviceModel_WindowsOS(
                                monitor.Monitor.Left,
                                monitor.Monitor.Top,
                                monitor.Monitor.Width,
                                monitor.Monitor.Height,
                                monitor.Flags == 1,
                                unityDisplayIndex,//int.Parse(monitor.DeviceName.ToLower().Replace("\\\\.\\display", "")),
                                displayDevice.DeviceString,
                                displayDevice.DeviceID,
                                displayDevice.DeviceKey,
                                graphicsDeviceName);
                            _displayDevices.Add(display);
                            monitorControlPanelIndex++;
                            Debug.Log(display);
                        }
                        return true;
                    }, IntPtr.Zero);
            }

            return _displayDevices;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFOEX
        {
            public uint Size;
            public RectNative Monitor;
            public RectNative WorkArea;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [Flags()]
        public enum DisplayDeviceStateFlags : int
        {
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            PrimaryDevice = 0x4,
            MirroringDriver = 0x8,
            VGACompatible = 0x10,
            Removable = 0x20,
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }
        #endregion
    }
}
#endif
