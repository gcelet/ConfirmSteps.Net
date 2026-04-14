namespace ConfirmSteps.Steps.Http.Rest;

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

using ConfirmSteps.Steps.Http.ResponseParsing;

/// <summary>
/// Represents an HTTP response with a JSON body.
/// </summary>
public sealed class HttpResponseJson : IJsonDocumentProvider, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseJson"/> class.
    /// </summary>
    /// <param name="innerResponse">The inner HTTP response message.</param>
    /// <param name="response">The parsed JSON document.</param>
    public HttpResponseJson(HttpResponseMessage? innerResponse, JsonDocument? response)
    {
        InnerResponse = innerResponse;
        Response = response;
        HeadersLazy = new(() =>
        {
          if (InnerResponse == null)
          {
            return null;
          }

          HttpResponseHeaders headers = InnerResponse.Headers;
          HttpContentHeaders? contentHeaders = InnerResponse.Content?.Headers;

          return new HttpResponseCombinedHeaders(headers, contentHeaders);
        });
    }

    private Lazy<HttpResponseCombinedHeaders?> HeadersLazy { get; }

    /// <summary>
    /// Gets the HTTP headers from the response.
    /// </summary>
    public HttpHeaders? Headers => HeadersLazy.Value;

    /// <summary>
    /// Gets a value indicating whether the HTTP response was successful.
    /// </summary>
    public bool IsSuccessStatusCode => InnerResponse?.IsSuccessStatusCode ?? false;

    /// <summary>
    /// Gets the parsed JSON document.
    /// </summary>
    public JsonDocument? Response { get; }

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public HttpStatusCode StatusCode => InnerResponse?.StatusCode ?? (HttpStatusCode)0;

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
