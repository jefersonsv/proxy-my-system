namespace ProxyMySystem
{
    public class UrlTemplate
    {
        public UrlTemplate(IUserData userData, bool clear)
        {
            this.ClearData = clear;
            if (userData == null)
                return;

            this.UserData = userData;
        }

        public bool ClearData { get; }
        public bool MustBeAuthenticated { get; }
        private IUserData UserData { get; }

        public string GetHost()
        {
            return UserData?.ProxyHost;
        }

        public string GetPort()
        {
            return UserData?.ProxyPort;
        }

        public string GetProxyExceptions()
        {
            return UserData?.ProxyExceptions;
        }


        public string GetProxySsl(bool noAuthenticated = false, bool forceClearPassword = false, bool ignoreDomain = false)
        {
            return GetProxy(noAuthenticated, forceClearPassword, ignoreDomain).Insert(4, "s");
        }

        public string GetProxyPassword(bool forceClearPassword = false)
        {
            if (forceClearPassword)
            {
                if (!string.IsNullOrEmpty(this.UserData.ProxyPassword))
                {
                    return this.UserData.ProxyPassword;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.UserData.ProxyPassword))
                {
                    var passwordEncoded = System.Web.HttpUtility.UrlEncode(this.UserData.ProxyPassword ?? string.Empty);
                    return passwordEncoded;
                }
            }

            return null;
        }

        public string GetProxyUsername(bool ignoreDomain = false)
        {
            if (ignoreDomain)
            {
                if (!string.IsNullOrEmpty(this.UserData.ProxyUsername))
                {
                    return this.UserData.ProxyUsername;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.UserData?.ProxyDomain) && !string.IsNullOrEmpty(this.UserData?.ProxyUsername))
                {
                    return $@"{this.UserData.ProxyDomain}\{this.UserData.ProxyUsername}";
                }
                else if (!string.IsNullOrEmpty(this.UserData?.ProxyUsername))
                {
                    return this.UserData.ProxyUsername;
                }
            }

            return null;
        }

        public string GetProxy(bool noAuthenticated = false, bool forceClearPassword = false, bool ignoreDomain = false)
        {
            System.UriBuilder builder = new System.UriBuilder();

            // host
            builder.Host = UserData.ProxyHost;

            // port
            if (!string.IsNullOrEmpty(UserData.ProxyPort))
                builder.Port = int.Parse(UserData.ProxyPort);
            else
                builder.Port = -1;

            if (!noAuthenticated)
            {
                builder.UserName = GetProxyUsername();
                builder.Password = GetProxyPassword(forceClearPassword);
            }

            return builder.ToString();
        }
    }
}