namespace ConfirmSteps.Steps.Http;

public class ExternalHttpClientProvider : IHttpClientProvider
{
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