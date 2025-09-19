using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;

namespace AISupporter.App.Helper
{
    public class ScreenCaptureHelper
    {
        #region Constants

        private const int SRCCOPY = 0x00CC0020;
        private const uint EDD_GET_DEVICE_INTERFACE_NAME = 0x00000001;
        private const uint DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001;
        private const uint DISPLAY_DEVICE_PRIMARY_DEVICE = 0x00000004;

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        #endregion

        #region Delegates

        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        #endregion

        #region DLL Imports

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight,
            IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        #endregion

        #region Screen Info Class

        public class ScreenInfo
        {
            public int Index { get; set; }
            public string DeviceName { get; set; } = string.Empty;
            public Rectangle Bounds { get; set; }
            public Rectangle WorkingArea { get; set; }
            public bool IsPrimary { get; set; }
            public IntPtr MonitorHandle { get; set; }

            public override string ToString()
            {
                return DeviceName;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Captures the entire virtual screen (all monitors combined)
        /// </summary>
        /// <returns>Screen capture as System.Drawing.Bitmap</returns>
        public static Bitmap CaptureAllScreens()
        {
            // Get virtual screen dimensions
            int virtualScreenLeft = (int)SystemParameters.VirtualScreenLeft;
            int virtualScreenTop = (int)SystemParameters.VirtualScreenTop;
            int virtualScreenWidth = (int)SystemParameters.VirtualScreenWidth;
            int virtualScreenHeight = (int)SystemParameters.VirtualScreenHeight;

            return CaptureScreenArea(virtualScreenLeft, virtualScreenTop, virtualScreenWidth, virtualScreenHeight);
        }

        /// <summary>
        /// Captures the primary screen
        /// </summary>
        /// <returns>Primary screen capture as System.Drawing.Bitmap</returns>
        public static Bitmap CapturePrimaryScreen()
        {
            int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            int screenHeight = (int)SystemParameters.PrimaryScreenHeight;

            return CaptureScreenArea(0, 0, screenWidth, screenHeight);
        }

        /// <summary>
        /// Gets information about all available screens
        /// </summary>
        /// <returns>List of ScreenInfo objects</returns>
        public static List<ScreenInfo> GetAllScreenInfo()
        {
            var screens = new List<ScreenInfo>();
            var displayNames = GetDisplayNames();
            int screenIndex = 0;

            MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                var monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);

                if (GetMonitorInfo(hMonitor, ref monitorInfo))
                {
                    var width = monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left;
                    var height = monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top;
                    var isPrimary = (monitorInfo.dwFlags & 1) == 1;

                    // Get the display name from our dictionary
                    string displayName = "Generic Monitor";
                    var deviceKeys = displayNames.Keys.ToArray();
                    if (screenIndex < deviceKeys.Length)
                    {
                        displayName = displayNames[deviceKeys[screenIndex]];
                    }

                    // Clean up the display name
                    displayName = CleanDisplayName(displayName);

                    // Format like OBS: "Monitor Name: 1920x1080 @0,0 (Primary Monitor)"
                    string deviceName = $"{displayName}: {width}x{height} @{monitorInfo.rcMonitor.Left},{monitorInfo.rcMonitor.Top}";
                    if (isPrimary)
                    {
                        deviceName += " (Primary Monitor)";
                    }

                    var screen = new ScreenInfo
                    {
                        Index = screenIndex++,
                        DeviceName = deviceName,
                        Bounds = new Rectangle(
                            monitorInfo.rcMonitor.Left,
                            monitorInfo.rcMonitor.Top,
                            width,
                            height
                        ),
                        WorkingArea = new Rectangle(
                            monitorInfo.rcWork.Left,
                            monitorInfo.rcWork.Top,
                            monitorInfo.rcWork.Right - monitorInfo.rcWork.Left,
                            monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top
                        ),
                        IsPrimary = isPrimary,
                        MonitorHandle = hMonitor
                    };

                    screens.Add(screen);
                }

                return true; // Continue enumeration
            };

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return screens;
        }

        /// <summary>
        /// Captures a specific screen by index
        /// </summary>
        /// <param name="screenIndex">Zero-based screen index</param>
        /// <returns>Screen capture as System.Drawing.Bitmap</returns>
        public static Bitmap CaptureSpecificScreen(int screenIndex)
        {
            var screens = GetAllScreenInfo();

            if (screenIndex < 0 || screenIndex >= screens.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(screenIndex),
                    $"Screen index {screenIndex} is out of range. Available screens: 0-{screens.Count - 1}");
            }

            var screen = screens[screenIndex];
            return CaptureScreenArea(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
        }

        /// <summary>
        /// Captures a specific screen area
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Screen capture as System.Drawing.Bitmap</returns>
        public static Bitmap CaptureScreenArea(int x, int y, int width, int height)
        {
            // Get the device context for the entire screen
            IntPtr desktopHandle = GetDesktopWindow();
            IntPtr desktopDC = GetWindowDC(desktopHandle);
            IntPtr memoryDC = CreateCompatibleDC(desktopDC);
            IntPtr bitmap = CreateCompatibleBitmap(desktopDC, width, height);
            IntPtr oldBitmap = SelectObject(memoryDC, bitmap);

            // Copy the screen area to our bitmap
            BitBlt(memoryDC, 0, 0, width, height, desktopDC, x, y, SRCCOPY);

            // Create a .NET Bitmap from the handle
            Bitmap screenCapture = Image.FromHbitmap(bitmap);

            // Clean up
            SelectObject(memoryDC, oldBitmap);
            DeleteObject(bitmap);
            DeleteDC(memoryDC);
            ReleaseDC(desktopHandle, desktopDC);

            return screenCapture;
        }

        /// <summary>
        /// Captures the screen and saves it to a file
        /// </summary>
        /// <param name="filePath">Path where to save the screenshot</param>
        /// <param name="format">Image format (PNG, JPEG, etc.)</param>
        public static void CaptureAllScreensToFile(string filePath, ImageFormat? format = null) // Fix CS8625
        {
            format ??= ImageFormat.Png; // Use null-coalescing assignment

            using (Bitmap bitmap = CaptureAllScreens())
            {
                bitmap.Save(filePath, format);
            }
        }

        /// <summary>
        /// Captures a specific screen and saves it to a file
        /// </summary>
        /// <param name="screenIndex">Screen index to capture</param>
        /// <param name="filePath">Path where to save the screenshot</param>
        /// <param name="format">Image format (PNG, JPEG, etc.)</param>
        public static void CaptureSpecificScreenToFile(int screenIndex, string filePath, ImageFormat? format = null) // Fix CS8625
        {
            format ??= ImageFormat.Png; // Use null-coalescing assignment

            using (Bitmap bitmap = CaptureSpecificScreen(screenIndex))
            {
                bitmap.Save(filePath, format);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets display names using EnumDisplayDevices
        /// </summary>
        /// <returns>Dictionary of device names</returns>
        private static Dictionary<string, string> GetDisplayNames()
        {
            var displayNames = new Dictionary<string, string>();
            var displayDevice = new DISPLAY_DEVICE();
            displayDevice.cb = Marshal.SizeOf(displayDevice);

            for (uint deviceNum = 0; EnumDisplayDevices(null, deviceNum, ref displayDevice, 0); deviceNum++)
            {
                if ((displayDevice.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP) != 0)
                {
                    // Get the monitor name for this display adapter
                    var monitorDevice = new DISPLAY_DEVICE();
                    monitorDevice.cb = Marshal.SizeOf(monitorDevice);

                    if (EnumDisplayDevices(displayDevice.DeviceName, 0, ref monitorDevice, EDD_GET_DEVICE_INTERFACE_NAME))
                    {
                        displayNames[displayDevice.DeviceName] = monitorDevice.DeviceString ?? "Unknown Monitor";
                    }
                    else
                    {
                        // Fallback to adapter name if monitor name not available
                        displayNames[displayDevice.DeviceName] = displayDevice.DeviceString ?? "Unknown Display";
                    }
                }
            }

            return displayNames;
        }

        /// <summary>
        /// Cleans up display names by removing unnecessary text
        /// </summary>
        /// <param name="name">Raw display name</param>
        /// <returns>Cleaned display name</returns>
        private static string CleanDisplayName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Unknown Monitor";

            // Remove common unnecessary text
            name = name.Replace("Generic PnP Monitor", "Generic Monitor")
                      .Replace("(Digital)", "")
                      .Replace("(Analog)", "")
                      .Trim();

            // If the name is still generic, try to make it more descriptive
            if (name.Contains("Generic"))
            {
                name = name.Replace("Generic Monitor", "Display");
            }

            return name;
        }

        #endregion
    }
}
