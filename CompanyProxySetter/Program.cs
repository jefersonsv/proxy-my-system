using CompanyProxySetter.OperationalSystem;
using Config.Net;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Linq;
using System.IO;
using System.Text;

namespace CompanyProxySetter
{
    public class Program
    {
        public static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            if (!Windows.IsAdministrator())
            {
                logger.Error("You must run this with Administrator permission");
                Environment.Exit(1);
            }

            logger.Info("Getting profile");
            var userSettings = GettingProfileUserData();
            if (userSettings == null)
            {
                logger.Error("You must set user data settings using any bellow methods:");
                logger.Info("APP ARGUMENTS => aplication arguments");
                logger.Info("APP SETTINGS => CompanyProxySetter.exe.config");
                logger.Info("INI FILE => settings.ini");
                logger.Info("JSON FILE => settings.json");
                logger.Info("VARIABLE => environment variables");
                logger.Info("The most important parameter is: PROXY_HOST then the first data settings that there are this value will be used");
                Environment.Exit(2);
            }
            else
            {
                logger.Info("Starting process to configure proxy: " + userSettings.ProxyHost);
            }

            logger.Info("Clearing currently configuration");
            EnvironmentVariables.ClearHttpProxy(EnvironmentVariableTarget.Machine);
            EnvironmentVariables.ClearHttpsProxy(EnvironmentVariableTarget.Machine);
            LanSettings.ClearExceptions();

            logger.Info("Setting profile");
            SettingUserData(userSettings);

            logger.Info("The system has been configured");

            Environment.Exit(0);
        }

        private static IUserData GettingProfileUserData()
        {
            List<IUserData> settings = new List<IUserData>();
            settings.Add(new ConfigurationBuilder<IUserData>().UseCommandLineArgs().Build());
            settings.Add(new ConfigurationBuilder<IUserData>().UseAppConfig().Build());
            settings.Add(new ConfigurationBuilder<IUserData>().UseIniFile(System.IO.Path.Combine(Environment.CurrentDirectory, "settings.ini")).Build());
            settings.Add(new ConfigurationBuilder<IUserData>().UseJsonFile(System.IO.Path.Combine(Environment.CurrentDirectory, "settings.json")).Build());
            settings.Add(new ConfigurationBuilder<IUserData>().UseEnvironmentVariables().Build());

            return settings.FirstOrDefault(w => !string.IsNullOrEmpty(w.ProxyHost));
        }

        private static void SettingUserData(IUserData userData)
        {
            var passwordEncoded = System.Web.HttpUtility.UrlEncode(userData.ProxyPassword ?? string.Empty);

            // Configure control panel
            logger.Info("Setting control panel");
            LanSettings.SetProxy($"{userData.ProxyHost}:{userData.ProxyPort}");
            LanSettings.ClearExceptions();
            if (!string.IsNullOrEmpty(userData.ProxyExceptions))
            {
                LanSettings.AddExceptions($"{userData.ProxyExceptions}");
            }
            LanSettings.SetExceptionForLocal(true);

            // Configure environment variables
            logger.Info("Setting environment variable HTTP_PROXY and HTTPS_PROXY for current user");
            logger.Info("A lot of applications such as: Ruby Gem, Ionic, Cordova, etc will be work with this configuration");
            EnvironmentVariables.SetHttpProxy($"http://{userData.ProxyUsername}:{passwordEncoded}@{userData.ProxyHost}:{userData.ProxyPort}", EnvironmentVariableTarget.User);
            EnvironmentVariables.SetHttpsProxy($"http://{userData.ProxyUsername}:{passwordEncoded}@{userData.ProxyHost}:{userData.ProxyPort}", EnvironmentVariableTarget.User);

            // Configure GIT
            if (Msdos.IsInstalled("git"))
            {
                logger.Info("Setting proxy for GIT");
                Msdos.Run("git", $"config --global http.proxy http://{userData.ProxyUsername}:{passwordEncoded}@{userData.ProxyHost}:{userData.ProxyPort}");
            }

            // Configure CHOCOLATEY
            if (Msdos.IsInstalled("choco"))
            {
                logger.Info("Setting proxy for chocolatey");
                Msdos.Run("choco", $@"config set proxy http://{userData.ProxyHost}:{userData.ProxyPort}");
                Msdos.Run("choco", $@"config set proxyUser {userData.ProxyDomain}\{userData.ProxyUsername}");
                Msdos.Run("choco", $@"config set proxyPassword {userData.ProxyPassword}");
                Msdos.Run("choco", $@"config set proxyBypassList ""{userData.ProxyExceptions}");
                Msdos.Run("choco", $@"config set proxyBypassOnLocal true");
            }

            // Configure nuget
            if (Msdos.IsInstalled("nuget"))
            {
                logger.Info("Setting proxy for nuget");
                Msdos.Run("nuget", $@"config -set http_proxy=http://{userData.ProxyHost}:{userData.ProxyPort}");
                Msdos.Run("nuget", $@"config -set http_proxy.user={userData.ProxyDomain}\{userData.ProxyUsername}");
                Msdos.Run("nuget", $@"config -set http_proxy.password={userData.ProxyPassword}");
            }

            // Configure npm
            if (Msdos.IsInstalled("npm"))
            {
                logger.Info("Setting proxy for npm");
                Msdos.Run("npm", $@"npm config set proxy http://{userData.ProxyUsername}:{userData.ProxyPassword}@{userData.ProxyHost}:{userData.ProxyPort}");
                Msdos.Run("npm", $@"npm config set https-proxy http://{userData.ProxyUsername}:{userData.ProxyPassword}@{userData.ProxyHost}:{userData.ProxyPort}");
            }

            // Configure npm
            if (Msdos.IsInstalled("npm"))
            {
                logger.Info("Setting proxy for npm");
                Msdos.Run("npm", $@"npm config set proxy http://{userData.ProxyUsername}:{userData.ProxyPassword}@{userData.ProxyHost}:{userData.ProxyPort}");
                Msdos.Run("npm", $@"npm config set https-proxy http://{userData.ProxyUsername}:{userData.ProxyPassword}@{userData.ProxyHost}:{userData.ProxyPort}");
            }

            // Configure bower
            if (Msdos.IsInstalled("bower"))
            {
                logger.Info("Setting proxy for bower");
                Msdos.Run("npm", $@"npm config set proxy http://{userData.ProxyUsername}:{userData.ProxyPassword}@{userData.ProxyHost}:{userData.ProxyPort}");
                Msdos.Run("npm", $@"npm config set https-proxy http://{userData.ProxyUsername}:{userData.ProxyPassword}@{userData.ProxyHost}:{userData.ProxyPort}");
            }

            // Configure android sdk
            var androidSdkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".android");
            if (Directory.Exists(androidSdkPath))
            {
                logger.Info("Setting proxy for Android SDK");
                File.WriteAllText(Path.Combine(androidSdkPath, "androidtool.cfg"), RenderConfigFileAndroidSdk(userData));
            }

            // Configure gradle
            if (Msdos.IsInstalled("gradle"))
            {
                var gradlePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                logger.Info("Setting proxy for gradle");
                File.WriteAllText(Path.Combine(gradlePath, ".gradle.properties"), RenderConfigFileGradle(userData));
            }
        }

        private static string RenderConfigFileGradle(IUserData userData)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"systemProp.http.proxyHost=={userData.ProxyHost}");
            sb.AppendLine($"systemProp.http.proxyPort={userData.ProxyPort}");
            sb.AppendLine($"systemProp.http.nonProxyHosts={userData.ProxyExceptions}");
            sb.AppendLine($"systemProp.https.proxyHost=={userData.ProxyHost}");
            sb.AppendLine($"systemProp.https.proxyPort={userData.ProxyPort}");
            sb.AppendLine($"systemProp.https.nonProxyHosts={userData.ProxyExceptions}");
            return sb.ToString();
        }

        private static string RenderConfigFileAndroidSdk(IUserData userData)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"http.proxyHost={userData.ProxyHost}");
            sb.AppendLine($"http.proxyPort={userData.ProxyPort}");
            return sb.ToString();
        }
    }
}