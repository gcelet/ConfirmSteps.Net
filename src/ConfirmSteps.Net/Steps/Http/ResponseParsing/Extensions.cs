namespace ConfirmSteps.Steps.Http.ResponseParsing;

using ConfirmSteps.Steps.Http.Rest;
using Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static async Task<HttpResponseJson> ParseJson<T>(
        this StepContext<T> stepContext, HttpResponseMessage response, CancellationToken cancellationToken)
        where T : class
    {
        if (stepContext.TryGetItem(out HttpResponseJson? jsonResponse) && jsonResponse != null)
        {
            return jsonResponse;
        }

        IHttpResponseJsonParser httpResponseJsonParser =
            stepContext.Services.GetRequiredService<IHttpResponseJsonParser>();
        HttpResponseParseResult<HttpResponseJson> parseResult =
            await httpResponseJsonParser.TryParse(response, cancellationToken);

        if (!parseResult.Success)
        {
            if (parseResult.Exception != null)
            {
                throw parseResult.Exception;
            }

            throw new Exception("Http response parsing into json failed");
        }

        if (parseResult.Response == null)
        {
            throw new Exception("Http response parsing into json failed: json response is null");
        }

        stepContext.AddItem(parseResult.Response);

        return parseResult.Response;
    }

    public static async Task<RestApiResult> ParseRestApi<T>(
        this StepContext<T> stepContext, HttpResponseMessage response, CancellationToken cancellationToken)
        where T : class
    {
        HttpResponseJson jsonResponse =
            await stepContext.ParseJson(response, cancellationToken);
        IHttpResponseRestApiResultParser httpResponseRestApiResultParser =
            stepContext.Services.GetRequiredService<IHttpResponseRestApiResultParser>();
        HttpResponseParseResult<RestApiResult> parseResult =
            await httpResponseRestApiResultParser.TryParse(jsonResponse, cancellationToken);

        if (!parseResult.Success)
        {
            if (parseResult.Exception != null)
            {
                throw parseResult.Exception;
            }

            throw new Exception("Http response parsing into rest api result failed");
        }

        if (parseResult.Response == null)
        {
            throw new Exception("Http response parsing into rest api result failed: rest api result response is null");
        }

        stepContext.AddItem(parseResult.Response);

        return parseResult.Response;
    }

    public static async Task<RestApiResult<TResult>> ParseRestApi<T, TResult>(
        this StepContext<T> stepContext, HttpResponseMessage response, CancellationToken cancellationToken)
        where T : class
        where TResult : class
    {
        HttpResponseJson jsonResponse =
            await stepContext.ParseJson(response, cancellationToken);
        IHttpResponseRestApiResultParser httpResponseRestApiResultParser =
            stepContext.Services.GetRequiredService<IHttpResponseRestApiResultParser>();
        HttpResponseParseResult<RestApiResult<TResult>> parseResult =
            await httpResponseRestApiResultParser.TryParse<TResult>(jsonResponse, cancellationToken);

        if (!parseResult.Success)
        {
            if (parseResult.Exception != null)
            {
                throw parseResult.Exception;
            }

            throw new Exception("Http response parsing into rest api result failed");
        }

        if (parseResult.Response == null)
        {
            throw new Exception("Http response parsing into rest api result failed: rest api result response is null");
        }

        stepContext.AddItem(parseResult.Response);

        return parseResult.Response;
    }
}