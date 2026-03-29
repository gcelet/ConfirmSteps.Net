namespace ConfirmSteps.Steps.Http;

/// <summary>
/// Provides an implementation of <see cref="IHttpClientProvider"/> that uses an externally provided <see cref="HttpClient"/>.
/// </summary>
public class ExternalHttpClientProvider : IHttpClientProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalHttpClientProvider"/> class with the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
    public ExternalHttpClientProvider(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    private HttpClient HttpClient { get; }

    /// <inheritdoc />
    public HttpClient Provide()
    {
        return HttpClient;
    }
}
