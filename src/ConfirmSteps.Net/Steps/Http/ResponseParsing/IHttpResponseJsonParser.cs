namespace ConfirmSteps.Steps.Http.ResponseParsing;

using ConfirmSteps.Steps.Http.Rest;

public interface IHttpResponseJsonParser
{
    Task<HttpResponseParseResult<HttpResponseJson>> TryParse(HttpResponseMessage? httpResponseMessage,
        CancellationToken cancellationToken);
}