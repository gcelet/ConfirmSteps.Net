namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Net.Http.Headers;

/// <summary>
/// Represents a combined view of HTTP response headers, including both the main response headers and the content headers.
/// </summary>
public class HttpResponseCombinedHeaders : HttpHeaders
{
  /// <summary>
  /// Initializes a new instance of the <see cref="HttpResponseCombinedHeaders"/> class
  /// by combining the provided response headers and content headers.
  /// </summary>
  /// <param name="headers">The main HTTP response headers.</param>
  /// <param name="contentHeaders">The HTTP content headers, which may be null if there is no content.</param>
  public HttpResponseCombinedHeaders(HttpResponseHeaders headers, HttpContentHeaders? contentHeaders)
  {
    foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
    {
      foreach (string value in header.Value)
      {
        TryAddWithoutValidation(header.Key, value);
      }
    }

    if (contentHeaders != null)
    {
      foreach (KeyValuePair<string, IEnumerable<string>> header in contentHeaders)
      {
        foreach (string value in header.Value)
        {
          TryAddWithoutValidation(header.Key, value);
        }
      }
    }
  }
}
