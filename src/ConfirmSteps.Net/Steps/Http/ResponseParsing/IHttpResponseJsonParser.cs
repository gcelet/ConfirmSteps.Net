namespace ConfirmSteps.Steps.Http.ResponseParsing;

using ConfirmSteps.Steps.Http.Rest;

/// <summary>
/// Defines a parser for converting an <see cref="HttpResponseMessage"/> to an <see cref="HttpResponseJson"/>.
/// </summary>
public interface IHttpResponseJsonParser
{
    /// <summary>
    /// Attempts to parse the HTTP response message into a JSON response object.
    /// </summary>
    /// <param name="httpResponseMessage">The HTTP response message to parse.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A parse result containing the JSON response or failure information.</returns>
    Task<HttpResponseParseResult<HttpResponseJson>> TryParse(HttpResponseMessage? httpResponseMessage,
        CancellationToken cancellationToken);
}
