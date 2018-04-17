namespace ProxyAtWork
{
    public class UrlTemplate
    {
        public UrlTemplate(IUserData userData)
        {
            var passwordEncoded = System.Web.HttpUtility.UrlEncode(userData.ProxyPassword ?? string.Empty);
            this.HttpProxyEncodedPassword = $"http://{userData.ProxyUsername}:{passwordEncoded}@{userData.ProxyHost}:{userData.ProxyPort}";
            this.HttpsProxyEncodedPassword = $"https://{userData.ProxyUsername}:{passwordEncoded}@{userData.ProxyHost}:{userData.ProxyPort}";
            this.HttpProxyNormalPassword = $"http://{userData.ProxyUsername}:{userData.ProxyPassword}@{userData.ProxyHost}:{userData.ProxyPort}";
            this.HttpsProxyNormalPassword = $"https://{userData.ProxyUsername}:{userData.ProxyPassword}@{userData.ProxyHost}:{userData.ProxyPort}";
            this.HttpProxy = $"http://{userData.ProxyHost}:{userData.ProxyPort}";
            this.HttpsProxy = $"https://{userData.ProxyHost}:{userData.ProxyPort}";
            this.UserData = userData;
        }

        public string HttpProxy { get; }
        public string HttpProxyEncodedPassword { get; }
        public string HttpProxyNormalPassword { get; }
        public string HttpsProxy { get; }
        public string HttpsProxyEncodedPassword { get; }
        public string HttpsProxyNormalPassword { get; }
        public bool MustBeAuthenticated { get; }
        public IUserData UserData { get; }
    }
}