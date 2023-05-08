namespace ConfirmSteps.Steps.Http.ResponseParsing;

using ConfirmSteps.Steps.Http.Rest;

public interface IHttpResponseRestApiResultParser
{
    Task<HttpResponseParseResult<RestApiResult>> TryParse(HttpResponseJson? httpResponseJson,
        CancellationToken cancellationToken);

    Task<HttpResponseParseResult<RestApiResult<R>>> TryParse<R>(HttpResponseJson? httpResponseJson,
        CancellationToken cancellationToken)
        where R : class;
}