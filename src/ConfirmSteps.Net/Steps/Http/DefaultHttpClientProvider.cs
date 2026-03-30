namespace ConfirmSteps.Steps.Http;

using System.Net;

/// <summary>
/// Provides a default implementation of <see cref="IHttpClientProvider"/> and <see cref="ICookieContainerProvider"/>.
/// </summary>
public sealed class DefaultHttpClientProvider : IHttpClientProvider, ICookieContainerProvider
{
    #if NET9_0_OR_GREATER
    private readonly System.Threading.Lock lockObject = new();
    #else
    private readonly object lockObject = new();
    #endif

    private HttpClient? httpClient;

    private CookieContainer CookieContainer => CookieContainerLazy.Value;

    private Lazy<CookieContainer> CookieContainerLazy { get; } = new(() => new CookieContainer());

    private HttpClient HttpClient
    {
        get
        {
            if (httpClient == null)
            {
                lock (lockObject)
                {
                    if (httpClient == null)
                    {
                        HttpClientHandler handler = new()
                        {
                            CookieContainer = CookieContainer,
                        };
                        httpClient = new HttpClient(handler, true);
                    }
                }
            }

            return httpClient;
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
