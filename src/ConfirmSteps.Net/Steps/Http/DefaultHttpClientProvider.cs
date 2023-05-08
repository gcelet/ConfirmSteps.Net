namespace ConfirmSteps.Steps.Http;

using System.Net;

public sealed class DefaultHttpClientProvider : IHttpClientProvider, ICookieContainerProvider
{
    private readonly object lockObject = new();

    private HttpClient? _httpClient;

    private CookieContainer CookieContainer => CookieContainerLazy.Value;

    private Lazy<CookieContainer> CookieContainerLazy { get; } = new(() => new CookieContainer());

    private HttpClient HttpClient
    {
        get
        {
            if (_httpClient == null)
            {
                lock (lockObject)
                {
                    if (_httpClient == null)
                    {
                        HttpClientHandler handler = new()
                        {
                            CookieContainer = CookieContainer,
                        };
                        _httpClient = new HttpClient(handler, true);
                    }
                }
            }

            return _httpClient;
        }
    }

    /// <inheritdoc />
    HttpClient IHttpClientProvider.Provide()
    {
        return HttpClient;
    }

    /// <inheritdoc />
    CookieContainer ICookieContainerProvider.Provide()
    {
        return CookieContainer;
    }
}