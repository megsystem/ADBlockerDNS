using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace AdBlocker
{
    internal class DarkTitleBar
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr,
            ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        internal static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

        public static bool IsSystemThemeDark()
        {
            const string registryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKeyPath))
            {
                if (key != null)
                {
                    // The value "AppsUseLightTheme" is 0 for dark mode and 1 for light mode.
                    object value = key.GetValue("AppsUseLightTheme");
                    if (value != null && value is int intValue)
                    {
                        return intValue == 0;
                    }
                }
            }
            // If we can't determine, default to light theme.
            return false;
        }
    }
}
