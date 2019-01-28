using CommandLineParser.Arguments;
using Config.Net;
using CredentialManagement;
using Newtonsoft.Json.Linq;
using ProxyAtWork.OperationalSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProxyAtWork
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

            var parser = new CommandLineParser.CommandLineParser();
            SwitchArgument clear = new SwitchArgument('c', "clear", "Clear the configuration", false);
            parser.Arguments.Add(clear);
            parser.ParseCommandLine(args);

            logger.Info("Getting profile");
            var userSettings = GettingProfileUserData();

            if (!clear.Value)
            {
                if (userSettings == null)
                {
                    logger.Error("You must set user data settings using any bellow methods:");
                    logger.Info("APP ARGUMENTS => aplication arguments");
                    logger.Info("APP SETTINGS => ProxyAtWork.exe.config");
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
            }
            else
            {
                logger.Info("Starting clearing process");
            }

            var urlTemplate = new UrlTemplate(userSettings, clear.Value);

            ConfigInternetSettings(urlTemplate);
            ConfigWindowsCredential(urlTemplate);
            ConfigEnvironmentVariable(urlTemplate);
            ConfigNpm(urlTemplate);
            ConfigGit(urlTemplate);
            ConfigChocolatey(urlTemplate);
            //ConfigYarn(urlTemplate);
            ConfigNuget(urlTemplate);
            ConfigBower(urlTemplate);
            ConfigAndroidSdk(urlTemplate);
            ConfigGradle(urlTemplate);

            // Configure cntlm on localhost
            //var cntlmPath = Path.Combine(Environment.CurrentDirectory, "Lib", "cntlm-0.92.3");
            //var cntlmFiles = Directory.GetFiles(cntlmPath);
            //if (cntlmFiles.Contains(Path.Combine(cntlmPath, "cygrunsrv.exe")) && cntlmFiles.Contains(Path.Combine(cntlmPath, "cntlm.exe")))
            //{
            //    logger.Info("Setting cntlm on localhost");

            //    var existService = Msdos.Run(Path.Combine(cntlmPath, "cygrunsrv"), $@"--query cntlm");
            //    if (!existService.StandardError.Contains("The specified service does not exist as an installed service"))
            //    {
            //        // remove cntlm
            //        var remove = Msdos.Run(Path.Combine(cntlmPath, "cygrunsrv"), $@"--remove cntlm");
            //    }

            //    // Install
            //    var result = Msdos.Run(Path.Combine(cntlmPath, "cygrunsrv"), $@"--install cntlm --path {Path.Combine(cntlmPath, "cntlm.exe")}");
            //    var start = Msdos.Run(Path.Combine(cntlmPath, "cygrunsrv"), $@"--start cntlm");
            //}

            if (clear.Value)
            {
                logger.Info("The system has been cleared");
            }
            else
            {
                logger.Info("The system has been configured");
            }

            logger.Info("Please, close all instances of Command Prompt or any program that use proxy and open again to reload newest settings");

            Environment.Exit(0);
        }

        private static void ConfigAndroidSdk(UrlTemplate urlTemplate)
        {
            // Configure android sdk
            var androidSdkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".android");
            if (Directory.Exists(androidSdkPath))
            {
                if (urlTemplate.ClearData)
                {
                    logger.Info("Clearing proxy for Android SDK");
                    if (File.Exists(Path.Combine(androidSdkPath, "androidtool.cfg")))
                    {
                        File.Delete(Path.Combine(androidSdkPath, "androidtool.cfg"));
                    }
                }
                else
                {
                    logger.Info("Setting proxy for Android SDK");

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"http.proxyHost={urlTemplate.GetHost()}");
                    sb.AppendLine($"http.proxyPort={urlTemplate.GetPort()}");

                    File.WriteAllText(Path.Combine(androidSdkPath, "androidtool.cfg"), sb.ToString());
                }
            }
        }

        private static void ConfigBower(UrlTemplate urlTemplate)
        {
            // Configure bower
            if (Msdos.IsInstalled("bower.cmd"))
            {
                if (urlTemplate.ClearData)
                {
                    logger.Info("Clearing proxy for bower");
                    Msdos.Run("npm.cmd", $@"config delete proxy");
                    Msdos.Run("npm.cmd", $@"config delete https-proxy");
                }
                else
                {
                    logger.Info("Setting proxy for bower");
                    Msdos.Run("npm.cmd", $@"npm config set proxy {urlTemplate.GetProxy()}");
                    Msdos.Run("npm.cmd", $@"npm config set https-proxy {urlTemplate.GetProxySsl()}");

                    // Create bowerrc file
                    var bowerrc = Path.Combine(Windows.GetCurrentUserFolder(), ".bowerrc");
                    if (!File.Exists(bowerrc))
                    {
                        // create settings
                        var obj = new JObject();
                        obj.Add("proxy", urlTemplate.GetProxy());
                        obj.Add("https-proxy", urlTemplate.GetProxySsl());

                        System.IO.File.WriteAllText(bowerrc, obj.ToString());
                    }
                    else
                    {
                        // update settings
                        var content = System.IO.File.ReadAllText(bowerrc);
                        var obj = JObject.Parse(content);
                        obj["proxy"] = urlTemplate.GetProxy();
                        obj["https-proxy"] = urlTemplate.GetProxySsl();

                        System.IO.File.WriteAllText(bowerrc, obj.ToString());
                    }
                }
            }
        }

        private static void ConfigChocolatey(UrlTemplate urlTemplate)
        {
            // Configure CHOCOLATEY
            if (Msdos.IsInstalled("choco.exe"))
            {
                if (urlTemplate.ClearData)
                {
                    logger.Info("Clearing proxy for chocolatey");
                    Msdos.Run("choco.exe", $@"config unset proxy");
                    Msdos.Run("choco.exe", $@"config unset proxyUser");
                    Msdos.Run("choco.exe", $@"config unset proxyPassword");
                    Msdos.Run("choco.exe", $@"config unset proxyBypassList");
                    Msdos.Run("choco.exe", $@"config unset proxyBypassOnLocal");
                }
                else
                {
                    logger.Info("Setting proxy for chocolatey");
                    Msdos.Run("choco.exe", $@"config set proxy {urlTemplate.GetProxy()}");

                    if (!string.IsNullOrEmpty(urlTemplate.GetProxyUsername()))
                        Msdos.Run("choco.exe", $@"config set proxyUser {urlTemplate.GetProxyUsername()}");

                    if (!string.IsNullOrEmpty(urlTemplate.GetProxyPassword()))
                        Msdos.Run("choco.exe", $@"config set proxyPassword {urlTemplate.GetProxyPassword()}");

                    if (!string.IsNullOrEmpty(urlTemplate.GetProxyExceptions()))
                        Msdos.Run("choco.exe", $@"config set proxyBypassList ""{urlTemplate.GetProxyExceptions()}");

                    Msdos.Run("choco.exe", $@"config set proxyBypassOnLocal true");
                }
            }
        }

        private static void ConfigEnvironmentVariable(UrlTemplate urlTemplate)
        {
            if (urlTemplate.ClearData)
            {
                logger.Info("Clearing environment variable HTTP_PROXY and HTTPS_PROXY for current user");
                EnvironmentVariables.ClearHttpProxy(EnvironmentVariableTarget.User);
                EnvironmentVariables.ClearHttpsProxy(EnvironmentVariableTarget.User);
            }
            else
            {
                // Configure environment variables
                logger.Info("Setting environment variable HTTP_PROXY and HTTPS_PROXY for current user");
                logger.Info("A lot of applications such as: Ruby Gem, Ionic, Cordova, etc will be work with this configuration");
                if (urlTemplate.MustBeAuthenticated)
                {
                    EnvironmentVariables.SetHttpProxy(urlTemplate.GetProxy(), EnvironmentVariableTarget.User);
                    EnvironmentVariables.SetHttpsProxy(urlTemplate.GetProxySsl(), EnvironmentVariableTarget.User);
                }
                else
                {
                    EnvironmentVariables.SetHttpProxy(urlTemplate.GetProxy(), EnvironmentVariableTarget.User);
                    EnvironmentVariables.SetHttpsProxy(urlTemplate.GetProxySsl(), EnvironmentVariableTarget.User);
                }
            }
        }

        private static void ConfigGit(UrlTemplate urlTemplate)
        {
            // Configure GIT
            if (Msdos.IsInstalled("git.exe"))
            {
                if (urlTemplate.ClearData)
                {
                    logger.Info("Clearing proxy for GIT");
                    Msdos.Run("git.exe", $"config --global --unset http.proxy");
                }
                else
                {
                    logger.Info("Setting proxy for GIT");
                    Msdos.Run("git.exe", $"config --global http.proxy {urlTemplate.GetProxy()}");
                    Msdos.Run("git.exe", $"config --global https.proxy {urlTemplate.GetProxySsl()}");
                }
            }
        }

        private static void ConfigGradle(UrlTemplate urlTemplate)
        {
            // Configure gradle
            if (Msdos.IsInstalled("gradle"))
            {
                var gradlePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

                if (urlTemplate.ClearData)
                {
                    logger.Info("Clearing proxy for gradle");
                    StringBuilder sb = new StringBuilder();
                    File.WriteAllText(Path.Combine(gradlePath, ".gradle.properties"), sb.ToString());
                }
                else
                {
                    logger.Info("Setting proxy for gradle");

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"systemProp.http.proxyHost=={urlTemplate.GetHost()}");
                    sb.AppendLine($"systemProp.http.proxyPort={urlTemplate.GetPort()}");
                    sb.AppendLine($"systemProp.http.nonProxyHosts={urlTemplate.GetProxyExceptions()}");
                    sb.AppendLine($"systemProp.https.proxyHost=={urlTemplate.GetHost()}");
                    sb.AppendLine($"systemProp.https.proxyPort={urlTemplate.GetPort()}");
                    sb.AppendLine($"systemProp.https.nonProxyHosts={urlTemplate.GetProxyExceptions()}");

                    File.WriteAllText(Path.Combine(gradlePath, ".gradle.properties"), sb.ToString());
                }
            }
        }

        private static void ConfigInternetSettings(UrlTemplate urlTemplate)
        {
            // Configure control panel
            if (urlTemplate.ClearData)
            {
                logger.Info("Clearing control panel");
                LanSettings.DisableProxy();
                LanSettings.ClearExceptions();
            }
            else
            {
                logger.Info("Setting control panel");
                LanSettings.SetProxy(urlTemplate.GetProxy(true));
                LanSettings.ClearExceptions();
                if (!string.IsNullOrEmpty(urlTemplate.GetProxyExceptions()))
                {
                    LanSettings.AddExceptions($"{urlTemplate.GetProxyExceptions()}");
                }
                LanSettings.SetExceptionForLocal(true);
            }
        }

        private static void ConfigNpm(UrlTemplate urlTemplate)
        {
            // Configure npm
            if (Msdos.IsInstalled("npm.cmd"))
            {
                
                if (urlTemplate.ClearData)
                {
                    logger.Info("Clearing proxy for npm");
                    Msdos.Run("npm.cmd", $@"config delete proxy");
                    Msdos.Run("npm.cmd", $@"config delete https-proxy");
                }
                else
                {
                    logger.Info("Setting proxy for npm");
                    Msdos.Run("npm.cmd", $@"config set proxy {urlTemplate.GetProxy()}");
                    Msdos.Run("npm.cmd", $@"config set https-proxy {urlTemplate.GetProxySsl()}");

                    var npmrc = Path.Combine(Windows.GetCurrentUserFolder(), ".npmrc");
                    // npmrc file is managed automatically
                }
            }
        }

        private static void ConfigNuget(UrlTemplate urlTemplate)
        {
            // Configure nuget
            if (Msdos.IsInstalled("nuget.exe"))
            {
                if (urlTemplate.ClearData)
                {
                    logger.Info("Clearing proxy for nuget");
                    Msdos.Run("nuget.exe", $@"config -set http_proxy=");
                    Msdos.Run("nuget.exe", $@"config -set http_proxy.user=");
                    Msdos.Run("nuget.exe", $@"config -set http_proxy.password=");
                }
                else
                {
                    logger.Info("Setting proxy for nuget");
                    Msdos.Run("nuget.exe", $@"config -set http_proxy={urlTemplate.GetProxy(true)}");
                    Msdos.Run("nuget.exe", $@"config -set http_proxy.user={urlTemplate.GetProxyUsername()}");
                    Msdos.Run("nuget.exe", $@"config -set http_proxy.password={urlTemplate.GetProxyPassword(true)}");
                }
            }
        }

        private static void ConfigWindowsCredential(UrlTemplate urlTemplate)
        {
            if (urlTemplate.ClearData)
            {
                if (!string.IsNullOrEmpty(urlTemplate.GetHost()))
                {
                    logger.Info("Clearing proxy windows credentials");
                    using (var credentials = new Credential())
                    {
                        logger.Info("Clearing Windows and Generic credentials");
                        credentials.Target = urlTemplate.GetProxy(true);
                        credentials.Delete();
                    }
                }
            }
            else
            {
                if (urlTemplate.MustBeAuthenticated)
                {
                    // Configure Windows and Generic credentials
                    using (var credentials = new Credential())
                    {
                        logger.Info("Setting Windows and Generic credentials");
                        credentials.Password = urlTemplate.GetProxyPassword();
                        credentials.Target = urlTemplate.GetProxy(true);
                        credentials.Username = urlTemplate.GetProxyUsername(true);
                        credentials.PersistanceType = PersistanceType.LocalComputer;
                        credentials.Type = CredentialType.Generic;
                        credentials.Save();

                        credentials.Type = CredentialType.DomainPassword;
                        credentials.Save();
                    }
                }
            }
        }

        private static void ConfigYarn(UrlTemplate urlTemplate)
        {
            // Configure yarn
            if (Msdos.IsInstalled("yarn.cmd"))
            {
                if (urlTemplate.ClearData)
                {
                    logger.Info("Clearing proxy for yarn");
                    Msdos.Run("yarn.cmd", $@"config delete proxy");
                    Msdos.Run("yarn.cmd", $@"config delete https-proxy");
                }
                else
                {
                    logger.Info("Setting proxy for yarn");
                    Msdos.Run("yarn.cmd", $@"npm config set proxy {urlTemplate.GetProxy()}");
                    Msdos.Run("yarn.cmd", $@"npm config set https-proxy {urlTemplate.GetProxySsl()}");
                }
            }
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

        private static string RenderConfigFileGradle(UrlTemplate urlTemplate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"systemProp.http.proxyHost=={urlTemplate.GetHost()}");
            sb.AppendLine($"systemProp.http.proxyPort={urlTemplate.GetPort()}");
            sb.AppendLine($"systemProp.http.nonProxyHosts={urlTemplate.GetProxyExceptions()}");
            sb.AppendLine($"systemProp.https.proxyHost=={urlTemplate.GetHost()}");
            sb.AppendLine($"systemProp.https.proxyPort={urlTemplate.GetPort()}");
            sb.AppendLine($"systemProp.https.nonProxyHosts={urlTemplate.GetProxyExceptions()}");
            return sb.ToString();
        }
    }
}