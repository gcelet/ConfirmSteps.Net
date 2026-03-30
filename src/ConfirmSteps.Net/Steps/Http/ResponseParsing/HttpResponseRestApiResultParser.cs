namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Text.Json;

using ConfirmSteps.Steps.Http.Problems;
using ConfirmSteps.Steps.Http.Rest;

/// <summary>
/// Default implementation of <see cref="IHttpResponseRestApiResultParser"/>.
/// </summary>
public class HttpResponseRestApiResultParser : IHttpResponseRestApiResultParser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseRestApiResultParser"/> class.
    /// </summary>
    /// <param name="options">The JSON serializer options.</param>
    /// <param name="httpResponseJsonParser">The JSON parser.</param>
    public HttpResponseRestApiResultParser(JsonSerializerOptions options, IHttpResponseJsonParser httpResponseJsonParser)
    {
        Options = options;
        HttpResponseJsonParser = httpResponseJsonParser;
    }

    private JsonSerializerOptions Options { get; }

    private IHttpResponseJsonParser HttpResponseJsonParser { get; }

    /// <inheritdoc />
    public Task<HttpResponseParseResult<RestApiResult>> TryParse(
        HttpResponseJson? httpResponseJson, CancellationToken cancellationToken)
    {
        if (httpResponseJson == null)
        {
            return Task.FromResult(HttpResponseParseResult<RestApiResult>.Failed);
        }

        try
        {
            RestApiResult restApiResult = httpResponseJson.IsSuccessStatusCode
                ? new RestApiResult(httpResponseJson, httpResponseJson.Response)
                : new RestApiResult(httpResponseJson, jsonProblem: httpResponseJson.Response);

            return Task.FromResult(new HttpResponseParseResult<RestApiResult>
            {
                Success = true,
                Response = restApiResult,
            });
        }
        catch (JsonException exception)
        {
            return Task.FromResult(new HttpResponseParseResult<RestApiResult>
            {
                Success = false,
                Exception = exception,
            });
        }
        catch
        {
            return Task.FromResult(HttpResponseParseResult<RestApiResult>.Failed);
        }
    }

    /// <inheritdoc />
    public async Task<HttpResponseParseResult<RestApiResult<TResponse>>> TryParse<TResponse>(
        HttpResponseJson? httpResponseJson, CancellationToken cancellationToken)
        where TResponse : class
    {
        HttpResponseParseResult<RestApiResult> parseResult = await TryParse(httpResponseJson, cancellationToken);

        if (!parseResult.Success || parseResult.Response == null)
        {
            return new HttpResponseParseResult<RestApiResult<TResponse>>
            {
                Success = false,
                Exception = parseResult.Exception,
            };
        }

        try
        {
            bool haveProblem = parseResult.Response.HaveProblem;
            TResponse? result = !haveProblem ? parseResult.Response.JsonResult!.Deserialize<TResponse>() : null;
            ProblemDetails? problem =
                haveProblem ? parseResult.Response.JsonProblem!.Deserialize<ProblemDetails>() : null;

            return new HttpResponseParseResult<RestApiResult<TResponse>>
            {
                Success = true,
                Response = new RestApiResult<TResponse>(httpResponseJson,
                    parseResult.Response.JsonResult, parseResult.Response.JsonProblem,
                    result, problem)
            };
        }
        catch (JsonException exception)
        {
            return new HttpResponseParseResult<RestApiResult<TResponse>>
            {
                Success = false,
                Exception = exception,
            };
        }
        catch
        {
            return HttpResponseParseResult<RestApiResult<TResponse>>.Failed;
        }
    }
}
