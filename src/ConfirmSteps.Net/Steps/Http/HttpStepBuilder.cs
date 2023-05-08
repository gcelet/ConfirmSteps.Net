namespace ConfirmSteps.Steps.Http;

using System.Text.Json;
using ConfirmSteps.Internal;
using ConfirmSteps.Steps.Http.RequestBuilding;
using ConfirmSteps.Steps.Http.ResponseParsing;
using ConfirmSteps.Steps.Http.Rest;
using Microsoft.Extensions.DependencyInjection;

public sealed class HttpStepBuilder<T> : IStepBuilder<T>
    where T : class
{
    public HttpStepBuilder(Func<RequestBuilder> requestBuilder)
    {
        RequestBuilder = requestBuilder;
    }

    private List<IHttpResponseExtractor<T>> Extractors { get; } = new();

    private Func<RequestBuilder> RequestBuilder { get; }

    private Func<HttpResponseMessage, StepContext<T>, CancellationToken, Task> VerifyFunc { get; set; } =
        (_, _, _) => Task.CompletedTask;

    public HttpStepBuilder<T> Extract(Action<HttpResponseExtractionBuilder<T>> extract)
    {
        HttpResponseExtractionBuilder<T> httpResponseExtractionBuilder = new();

        extract(httpResponseExtractionBuilder);

        Extractors.AddRange(((IHttpResponseExtractorProvider<T>)httpResponseExtractionBuilder).Provide());

        return this;
    }

    public HttpStepBuilder<T> Verify(Action<HttpResponseMessage, StepContext<T>> verify)
    {
        VerifyFunc = (response, stepContext, _) =>
        {
            verify(response, stepContext);
            return Task.CompletedTask;
        };

        return this;
    }

    public HttpStepBuilder<T> Verify(Func<HttpResponseMessage, StepContext<T>, CancellationToken, Task> verify)
    {
        VerifyFunc = verify;

        return this;
    }

    public HttpStepBuilder<T> VerifyJson(Action<HttpResponseJson, StepContext<T>> verify)
    {
        VerifyFunc = async (response, stepContext, ct) =>
        {
            HttpResponseJson jsonResponse = await stepContext.ParseJson(response, ct);

            verify(jsonResponse, stepContext);
        };

        return this;
    }

    public HttpStepBuilder<T> VerifyJson(Func<HttpResponseJson, StepContext<T>, CancellationToken, Task> verify)
    {
        VerifyFunc = async (response, stepContext, ct) =>
        {
            HttpResponseJson jsonResponse = await stepContext.ParseJson(response, ct);

            await verify(jsonResponse, stepContext, ct);
        };

        return this;
    }

    public HttpStepBuilder<T> VerifyRestApiResult(Action<RestApiResult, StepContext<T>> verify)
    {
        VerifyFunc = async (response, stepContext, ct) =>
        {
            RestApiResult restApiResult = await stepContext.ParseRestApi(response, ct);

            verify(restApiResult, stepContext);
        };

        return this;
    }

    public HttpStepBuilder<T> VerifyRestApiResult(Func<RestApiResult, StepContext<T>, CancellationToken, Task> verify)
    {
        VerifyFunc = async (response, stepContext, ct) =>
        {
            RestApiResult restApiResult = await stepContext.ParseRestApi(response, ct);

            await verify(restApiResult, stepContext, ct);
        };

        return this;
    }

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