namespace ConfirmSteps.Steps.Http.ResponseParsing;

using ConfirmSteps.Steps.Http.Rest;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for HTTP response parsing within a step context.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Parses the HTTP response as JSON and caches it in the step context.
    /// </summary>
    /// <typeparam name="T">The type of the scenario data.</typeparam>
    /// <param name="stepContext">The step context.</param>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed <see cref="HttpResponseJson"/>.</returns>
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

    /// <summary>
    /// Parses the HTTP response as a REST API result and caches it in the step context.
    /// </summary>
    /// <typeparam name="T">The type of the scenario data.</typeparam>
    /// <param name="stepContext">The step context.</param>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed <see cref="RestApiResult"/>.</returns>
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

    /// <summary>
    /// Parses the HTTP response as a typed REST API result and caches it in the step context.
    /// </summary>
    /// <typeparam name="T">The type of the scenario data.</typeparam>
    /// <typeparam name="TResult">The type of the expected result data.</typeparam>
    /// <param name="stepContext">The step context.</param>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed <see cref="RestApiResult{TResult}"/>.</returns>
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
