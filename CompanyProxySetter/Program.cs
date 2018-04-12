using Config.Net;
using System;

namespace CompanyProxySetter
{
    public class Program
    {
        public static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            logger.Info("Getting profile");
            var userSettings = GettingProfileUserData();

            logger.Info("Starting process");

            logger.Info("Clearing currenty configuration");
            Internet.EnvironmentVariables.ClearHttpProxy(EnvironmentVariableTarget.Machine);
            Internet.EnvironmentVariables.ClearHttpsProxy(EnvironmentVariableTarget.Machine);
            Internet.LanSettings.ClearExceptions();

            logger.Info("Setting profile");
            SettingUserData(userSettings);
        }

        private static IUserData GettingProfileUserData()
        {
            IUserData settings = new ConfigurationBuilder<IUserData>()
               .UseCommandLineArgs()
               .UseAppConfig()
               .UseIniFile(@".\App.ini")
               .UseJsonFile(@".\App.json")
               .UseEnvironmentVariables()
               .Build();

            return settings;
        }

        private static void SettingUserData(IUserData userData)
        {
            var passwordEncoded = System.Web.HttpUtility.UrlEncode(userData.ProxyPassword ?? string.Empty);

            //Log.Info("Setando proxy no painel de controle");
            //Internet.LanSettings.SetProxy($"{PROXY_HOST}:{PROXY_PORT}");
            //Log.Info("Setando proxy na variavel de ambiente HTTP_PROXY");
            //ProxyManager.Internet.EnvironmentVariables.SetHttpProxy($"http://{USERNAME}:{PASSWORD_ENCODED}@{PROXY_HOST}:{PROXY_PORT}", EnvironmentVariableTarget.User);
            //Log.Info("Setando proxy na variavel de ambiente HTTPS_PROXY");
            //ProxyManager.Internet.EnvironmentVariables.SetHttpsProxy($"http://{USERNAME}:{PASSWORD_ENCODED}@{PROXY_HOST}:{PROXY_PORT}", EnvironmentVariableTarget.User);

            //// Configurar GIT
            //Log.Info("Limpando variavel http.proxy no git config");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("git", "config --global http.proxy \"\"", null, true);
            ///*
            //Log.Information("Removendo variavel http.proxy no git config");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("git", "config --global --unset http.proxy", null, true);
            //Log.Information("Removendo variavel https.proxy no git config");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("git", "config --global --unset https.proxy", null, true);
            //*/

            //// Configurar CHOCOLATEY
            //Log.Info("Setando proxy no chocolatey");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("choco", $@"config set proxy http://{PROXY_HOST}:{PROXY_PORT}", null, true);
            //Log.Info("Setando usuario de proxy no chocolatey");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("choco", $@"config set proxyUser {PASSWORD}\{USERNAME}", null, true);
            //Log.Info("Setando senha do usuario de proxy no chocolatey");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("choco", $@"config set proxyPassword {PASSWORD}", null, true);
            //Log.Info("Setando a lista de bypass no chocolatey");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("choco", $@"config set proxyBypassList ""{PROXY_EXCEPTIONS}""", null, true);
            //Log.Info("Setando o bypass localhost na lista de bypass");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("choco", $@"config set proxyBypassOnLocal true", null, true);

            //// Configurar Nuget (Browse package pelo VS e também no DOS)
            //Log.Info("Setando proxy no nuget");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("nuget", $@"config -set http_proxy=http://{PROXY_HOST}:{PROXY_PORT}", null, true);
            //Log.Info("Setando usuario de proxy no nuget");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("nuget", $@"config -set http_proxy.user={DOMAIN}\{USERNAME}", null, true);
            //Log.Info("Setando senha do usuario de proxy no nuget");
            //Dev.Essential.MsDos.ExecuteProgram.ExecuteAndWait("nuget", $@"config -set http_proxy.password={PASSWORD}", null, true);
        }
    }
}