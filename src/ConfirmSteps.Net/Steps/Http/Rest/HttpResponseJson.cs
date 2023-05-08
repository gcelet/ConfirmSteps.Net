namespace ConfirmSteps.Steps.Http.Rest;

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

public sealed class HttpResponseJson : IJsonDocumentProvider, IDisposable
{
    public HttpResponseJson(HttpResponseMessage? innerResponse, JsonDocument? response)
    {
        InnerResponse = innerResponse;
        Response = response;
    }

    public HttpResponseHeaders? Headers => InnerResponse?.Headers;

    public bool IsSuccessStatusCode => InnerResponse?.IsSuccessStatusCode ?? false;

    public JsonDocument? Response { get; }

    public HttpStatusCode StatusCode => InnerResponse?.StatusCode ?? 0;

    private HttpResponseMessage? InnerResponse { get; }

    /// <inheritdoc />
    JsonDocument? IJsonDocumentProvider.JsonDocument => Response;

    /// <inheritdoc />
    public void Dispose()
    {
        Response?.Dispose();
        InnerResponse?.Dispose();
    }
}