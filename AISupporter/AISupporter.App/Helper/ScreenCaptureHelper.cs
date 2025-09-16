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

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        #endregion

        #region Screen Info Class

        public class ScreenInfo
        {
            public int Index { get; set; }
            public string DeviceName { get; set; }
            public Rectangle Bounds { get; set; }
            public Rectangle WorkingArea { get; set; }
            public bool IsPrimary { get; set; }
            public IntPtr MonitorHandle { get; set; }

            public override string ToString()
            {
                return $"Screen {Index}: {Bounds.Width}x{Bounds.Height} at ({Bounds.X}, {Bounds.Y})" +
                       (IsPrimary ? " [Primary]" : "");
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
            int screenIndex = 0;

            MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                var monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);

                if (GetMonitorInfo(hMonitor, ref monitorInfo))
                {
                    var screen = new ScreenInfo
                    {
                        Index = screenIndex++,
                        DeviceName = $"Display{screenIndex}",
                        Bounds = new Rectangle(
                            monitorInfo.rcMonitor.Left,
                            monitorInfo.rcMonitor.Top,
                            monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left,
                            monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top
                        ),
                        WorkingArea = new Rectangle(
                            monitorInfo.rcWork.Left,
                            monitorInfo.rcWork.Top,
                            monitorInfo.rcWork.Right - monitorInfo.rcWork.Left,
                            monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top
                        ),
                        IsPrimary = (monitorInfo.dwFlags & 1) == 1, // MONITORINFOF_PRIMARY = 1
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
        public static void CaptureAllScreensToFile(string filePath, ImageFormat format = null)
        {
            format = format ?? ImageFormat.Png;

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
        public static void CaptureSpecificScreenToFile(int screenIndex, string filePath, ImageFormat format = null)
        {
            format = format ?? ImageFormat.Png;

            using (Bitmap bitmap = CaptureSpecificScreen(screenIndex))
            {
                bitmap.Save(filePath, format);
            }
        }

        #endregion
    }
}
