namespace CompanyProxySetter
{
    public interface IUserData
    {
        string ProxyDomain { get; }
        string ProxyExceptions { get; }
        string ProxyHost { get; }
        string ProxyPassword { get; }
        string ProxyPort { get; }
        string ProxyUsername { get; }
    }
}