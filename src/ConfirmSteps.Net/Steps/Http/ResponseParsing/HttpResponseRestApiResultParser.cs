namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Text.Json;
using ConfirmSteps.Steps.Http.Problems;
using ConfirmSteps.Steps.Http.Rest;

public class HttpResponseRestApiResultParser : IHttpResponseRestApiResultParser
{
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
    public async Task<HttpResponseParseResult<RestApiResult<R>>> TryParse<R>(
        HttpResponseJson? httpResponseJson, CancellationToken cancellationToken)
        where R : class
    {
        HttpResponseParseResult<RestApiResult> parseResult = await TryParse(httpResponseJson, cancellationToken);

        if (!parseResult.Success || parseResult.Response == null)
        {
            return new HttpResponseParseResult<RestApiResult<R>>
            {
                Success = false,
                Exception = parseResult.Exception,
            };
        }

        try
        {
            bool haveProblem = parseResult.Response.HaveProblem;
            R? result = !haveProblem ? parseResult.Response.JsonResult!.Deserialize<R>() : null;
            ProblemDetails? problem =
                haveProblem ? parseResult.Response.JsonProblem!.Deserialize<ProblemDetails>() : null;

            return new HttpResponseParseResult<RestApiResult<R>>
            {
                Success = true,
                Response = new RestApiResult<R>(httpResponseJson,
                    parseResult.Response.JsonResult, parseResult.Response.JsonProblem,
                    result, problem)
            };
        }
        catch (JsonException exception)
        {
            return new HttpResponseParseResult<RestApiResult<R>>
            {
                Success = false,
                Exception = exception,
            };
        }
        catch
        {
            return HttpResponseParseResult<RestApiResult<R>>.Failed;
        }
    }
}