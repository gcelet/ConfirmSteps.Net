namespace ConfirmSteps.Steps.Http;

using System.Text.Json;

using ConfirmSteps.Internal;
using ConfirmSteps.Steps.Http.RequestBuilding;
using ConfirmSteps.Steps.Http.ResponseParsing;
using ConfirmSteps.Steps.Http.Rest;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides a builder for creating an HTTP-based step.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public sealed class HttpStepBuilder<T> : IStepBuilder<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpStepBuilder{T}"/> class.
    /// </summary>
    /// <param name="requestBuilder">A function that returns a <see cref="RequestBuilder"/> configured for the request.</param>
    public HttpStepBuilder(Func<RequestBuilder> requestBuilder)
    {
        RequestBuilder = requestBuilder;
    }

    private List<IHttpResponseExtractor<T>> Extractors { get; } = new();

    private Func<RequestBuilder> RequestBuilder { get; }

    private Func<HttpResponseMessage, StepContext<T>, CancellationToken, Task> VerifyFunc { get; set; } =
        (_, _, _) => Task.CompletedTask;

    /// <summary>
    /// Configures data extraction from the HTTP response.
    /// </summary>
    /// <param name="extract">An action to configure the extraction using <see cref="HttpResponseExtractionBuilder{T}"/>.</param>
    /// <returns>The current <see cref="HttpStepBuilder{T}"/> for fluent chaining.</returns>
    public HttpStepBuilder<T> Extract(Action<HttpResponseExtractionBuilder<T>> extract)
    {
        HttpResponseExtractionBuilder<T> httpResponseExtractionBuilder = new();

        extract(httpResponseExtractionBuilder);

        Extractors.AddRange(((IHttpResponseExtractorProvider<T>)httpResponseExtractionBuilder).Provide());

        return this;
    }

    /// <summary>
    /// Configures the synchronous verification logic for the HTTP response.
    /// </summary>
    /// <param name="verify">The verification action.</param>
    /// <returns>The current <see cref="HttpStepBuilder{T}"/> for fluent chaining.</returns>
    public HttpStepBuilder<T> Verify(Action<HttpResponseMessage, StepContext<T>> verify)
    {
        VerifyFunc = (response, stepContext, _) =>
        {
            verify(response, stepContext);
            return Task.CompletedTask;
        };

        return this;
    }

    /// <summary>
    /// Configures the asynchronous verification logic for the HTTP response.
    /// </summary>
    /// <param name="verify">The asynchronous verification function.</param>
    /// <returns>The current <see cref="HttpStepBuilder{T}"/> for fluent chaining.</returns>
    public HttpStepBuilder<T> Verify(Func<HttpResponseMessage, StepContext<T>, CancellationToken, Task> verify)
    {
        VerifyFunc = verify;

        return this;
    }

    /// <summary>
    /// Configures the synchronous verification logic for a JSON response.
    /// </summary>
    /// <param name="verify">The verification action.</param>
    /// <returns>The current <see cref="HttpStepBuilder{T}"/> for fluent chaining.</returns>
    public HttpStepBuilder<T> VerifyJson(Action<HttpResponseJson, StepContext<T>> verify)
    {
        VerifyFunc = async (response, stepContext, ct) =>
        {
            HttpResponseJson jsonResponse = await stepContext.ParseJson(response, ct);

            verify(jsonResponse, stepContext);
        };

        return this;
    }

    /// <summary>
    /// Configures the asynchronous verification logic for a JSON response.
    /// </summary>
    /// <param name="verify">The asynchronous verification function.</param>
    /// <returns>The current <see cref="HttpStepBuilder{T}"/> for fluent chaining.</returns>
    public HttpStepBuilder<T> VerifyJson(Func<HttpResponseJson, StepContext<T>, CancellationToken, Task> verify)
    {
        VerifyFunc = async (response, stepContext, ct) =>
        {
            HttpResponseJson jsonResponse = await stepContext.ParseJson(response, ct);

            await verify(jsonResponse, stepContext, ct);
        };

        return this;
    }

    /// <summary>
    /// Configures the synchronous verification logic for a REST API result.
    /// </summary>
    /// <param name="verify">The verification action.</param>
    /// <returns>The current <see cref="HttpStepBuilder{T}"/> for fluent chaining.</returns>
    public HttpStepBuilder<T> VerifyRestApiResult(Action<RestApiResult, StepContext<T>> verify)
    {
        VerifyFunc = async (response, stepContext, ct) =>
        {
            RestApiResult restApiResult = await stepContext.ParseRestApi(response, ct);

            verify(restApiResult, stepContext);
        };

        return this;
    }

    /// <summary>
    /// Configures the asynchronous verification logic for a REST API result.
    /// </summary>
    /// <param name="verify">The asynchronous verification function.</param>
    /// <returns>The current <see cref="HttpStepBuilder{T}"/> for fluent chaining.</returns>
    public HttpStepBuilder<T> VerifyRestApiResult(Func<RestApiResult, StepContext<T>, CancellationToken, Task> verify)
    {
        VerifyFunc = async (response, stepContext, ct) =>
        {
            RestApiResult restApiResult = await stepContext.ParseRestApi(response, ct);

            await verify(restApiResult, stepContext, ct);
        };

        return this;
    }

    /// <summary>
    /// Configures the synchronous verification logic for a typed REST API result.
    /// </summary>
    /// <typeparam name="TResult">The type of the expected result.</typeparam>
    /// <param name="verify">The verification action.</param>
    /// <returns>The current <see cref="HttpStepBuilder{T}"/> for fluent chaining.</returns>
    public HttpStepBuilder<T> VerifyRestApiResult<TResult>(Action<RestApiResult<TResult>, StepContext<T>> verify)
        where TResult : class
    {
        VerifyFunc = async (response, stepContext, ct) =>
        {
            RestApiResult<TResult> restApiResult = await stepContext.ParseRestApi<T, TResult>(response, ct);

            verify(restApiResult, stepContext);
        };

        return this;
    }

    /// <summary>
    /// Configures the asynchronous verification logic for a typed REST API result.
    /// </summary>
    /// <typeparam name="TResult">The type of the expected result.</typeparam>
    /// <param name="verify">The asynchronous verification function.</param>
    /// <returns>The current <see cref="HttpStepBuilder{T}"/> for fluent chaining.</returns>
    public HttpStepBuilder<T> VerifyRestApiResult<TResult>(
        Func<RestApiResult<TResult>, StepContext<T>, CancellationToken, Task> verify)
        where TResult : class
    {
        VerifyFunc = async (response, stepContext, ct) =>
        {
            RestApiResult<TResult> restApiResult = await stepContext.ParseRestApi<T, TResult>(response, ct);

            await verify(restApiResult, stepContext, ct);
        };

        return this;
    }

    /// <inheritdoc />
    IStep<T> IStepBuilder<T>.Build()
    {
        RequestBuilder requestBuilder = RequestBuilder.Invoke();

        return new HttpStep<T>(requestBuilder, VerifyFunc, Extractors);
    }

    /// <inheritdoc />
    IServiceCollection IStepBuilder<T>.RegisterServices(IServiceCollection services)
    {
        if (services.All(d => d.ServiceType != typeof(IHttpClientProvider)))
        {
            services.AddSingleton<IHttpClientProvider>(_ => new DefaultHttpClientProvider());
        }

        if (services.All(d => d.ServiceType != typeof(JsonSerializerOptions)))
        {
            services.AddSingleton(HttpSettings.BuildJsonSerializerOptions());
        }

        if (services.All(d => d.ServiceType != typeof(IHttpResponseJsonParser)))
        {
            services.AddScoped<IHttpResponseJsonParser, HttpResponseJsonParser>();
        }

        if (services.All(d => d.ServiceType != typeof(IHttpResponseRestApiResultParser)))
        {
            services.AddScoped<IHttpResponseRestApiResultParser, HttpResponseRestApiResultParser>();
        }

        return services;
    }
}
