namespace ConfirmSteps.Steps.Http.ResponseParsing;

using ConfirmSteps.Steps.Http.Rest;

/// <summary>
/// Defines a parser for converting an <see cref="HttpResponseJson"/> to a <see cref="RestApiResult"/>.
/// </summary>
public interface IHttpResponseRestApiResultParser
{
    /// <summary>
    /// Attempts to parse the JSON response into a REST API result.
    /// </summary>
    /// <param name="httpResponseJson">The JSON response to parse.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A parse result containing the REST API result or failure information.</returns>
    Task<HttpResponseParseResult<RestApiResult>> TryParse(HttpResponseJson? httpResponseJson,
        CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to parse the JSON response into a typed REST API result.
    /// </summary>
    /// <typeparam name="R">The type of the expected result data.</typeparam>
    /// <param name="httpResponseJson">The JSON response to parse.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A parse result containing the typed REST API result or failure information.</returns>
    Task<HttpResponseParseResult<RestApiResult<R>>> TryParse<R>(HttpResponseJson? httpResponseJson,
        CancellationToken cancellationToken)
        where R : class;
}
