using Config.Net;

namespace CompanyProxySetter
{
    public interface IUserData
    {
        [Option(Alias = "PROXY_DOMAIN")]
        string ProxyDomain { get; }

        [Option(Alias = "PROXY_EXCEPTIONS")]
        string ProxyExceptions { get; }

        [Option(Alias = "PROXY_HOST")]
        string ProxyHost { get; }

        [Option(Alias = "PROXY_PASSWORD")]
        string ProxyPassword { get; }

        [Option(Alias = "PROXY_PORT")]
        string ProxyPort { get; }

        [Option(Alias = "PROXY_USERNAME")]
        string ProxyUsername { get; }
    }
}