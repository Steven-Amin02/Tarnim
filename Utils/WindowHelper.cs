using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Tarnim.Utils
{
    public static class WindowHelper
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        /// <summary>
        /// Forces the window title bar to use Dark Mode (or designated color if supported).
        /// </summary>
        public static bool UseImmersiveDarkMode(Window window, bool enabled)
        {
            if (window == null) return false;

            try
            {
                var windowInteropHelper = new WindowInteropHelper(window);
                var hwnd = windowInteropHelper.Handle;

                if (hwnd == IntPtr.Zero) return false;

                int useImmersiveDarkMode = enabled ? 1 : 0;

                // Try modern DWM attribute
                int result = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));

                if (result != 0)
                {
                    // Fallback for older Windows 10 versions
                    result = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int));
                }

                return result == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
