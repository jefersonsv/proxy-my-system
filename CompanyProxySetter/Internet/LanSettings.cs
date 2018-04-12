using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace CompanyProxySetter.Internet
{
    /// <summary>
    /// https://stackoverflow.com/questions/197725/programmatically-set-browser-proxy-settings-in-c-sharp
    /// </summary>
    public class LanSettings
    {
        public const int INTERNET_OPTION_REFRESH = 37;
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private static RegistryKey proxyRegistry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);

        private bool settingsReturn, refreshReturn;

        public static void AddExceptions(string exceptionByComma)
        {
            var value = GetExceptions();
            value += ";" + exceptionByComma;

            SetExceptions(value);
        }

        public static void ClearExceptions()
        {
            SetExceptions(string.Empty);
        }

        public static void DisableProxy()
        {
            proxyRegistry.SetValue("ProxyEnable", 0);
            RefreshSettings();
        }

        public static string GetExceptions()
        {
            return proxyRegistry.GetValue("ProxyOverride").ToString();
        }

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        public static void SetAutoConfig(string autoConfig)
        {
            proxyRegistry.SetValue("AutoConfigURL", autoConfig);
            RefreshSettings();
        }

        public static void SetExceptionForLocal(bool enable)
        {
            var value = GetExceptions();
            value = value.Replace("<local>", string.Empty);

            if (enable)
                value += ";<local>";

            SetExceptions(value);
        }

        public static void SetExceptions(string exceptionByComma)
        {
            proxyRegistry.SetValue("ProxyOverride", exceptionByComma);
            RefreshSettings();
        }

        public static void SetProxy(string proxy)
        {
            proxyRegistry.SetValue("ProxyEnable", 1);
            proxyRegistry.SetValue("ProxyServer", proxy);

            RefreshSettings();
        }

        public static void SetProxy(string httpProxy, string httpsProxy, string ftpProxy, string socksProxy)
        {
            httpProxy = string.IsNullOrEmpty(httpProxy) ? string.Empty : $"http={httpProxy}";
            httpsProxy = string.IsNullOrEmpty(httpsProxy) ? string.Empty : $"https={httpsProxy}";
            ftpProxy = string.IsNullOrEmpty(ftpProxy) ? string.Empty : $"ftp={ftpProxy}";
            socksProxy = string.IsNullOrEmpty(socksProxy) ? string.Empty : $"socks={socksProxy}";

            var fmt = $"{httpProxy};{httpsProxy};{ftpProxy};{socksProxy}";
            fmt = new Regex(";+").Replace(fmt, ";");
            fmt = new Regex("^;").Replace(fmt, string.Empty);
            fmt = new Regex(";$").Replace(fmt, string.Empty);
            SetProxy(fmt);
        }

        private static void RefreshSettings()
        {
            // These lines implement the Interface in the beginning of program
            // They cause the OS to refresh the settings, causing IP to realy update
            var settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            var refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
    }
}