using System;

namespace CompanyProxySetter.Internet
{
    public class EnvironmentVariables
    {
        public static void ClearHttpProxy(EnvironmentVariableTarget target)
        {
            Environment.SetEnvironmentVariable("HTTP_PROXY", null, target);
        }

        public static void ClearHttpsProxy(EnvironmentVariableTarget target)
        {
            Environment.SetEnvironmentVariable("HTTPS_PROXY", null, target);
        }

        public static void SetHttpProxy(string proxy, EnvironmentVariableTarget target)
        {
            Environment.SetEnvironmentVariable("HTTP_PROXY", proxy, target);
        }

        public static void SetHttpsProxy(string proxy, EnvironmentVariableTarget target)
        {
            Environment.SetEnvironmentVariable("HTTPS_PROXY", proxy, target);
        }
    }
}