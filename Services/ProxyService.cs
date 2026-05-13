namespace PythonEnvBuilder.Services;

public sealed class ProxyService
{
    public void Apply(string proxyUrl, bool httpProxy, bool httpsProxy, EnvironmentVariableTarget target)
    {
        if (httpProxy)
        {
            Environment.SetEnvironmentVariable("HTTP_PROXY", proxyUrl, target);
        }

        if (httpsProxy)
        {
            Environment.SetEnvironmentVariable("HTTPS_PROXY", proxyUrl, target);
        }
    }
}
