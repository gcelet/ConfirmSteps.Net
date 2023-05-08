namespace ConfirmSteps.Steps.Http;

using ConfirmSteps.Steps.Http.RequestBuilding;
using ConfirmSteps.Steps.Http.ResponseParsing;
using Microsoft.Extensions.DependencyInjection;

public sealed class HttpStep<T> : Step<T>
    where T : class
{
    public HttpStep(RequestBuilder requestBuilder,
        Func<HttpResponseMessage, StepContext<T>, CancellationToken, Task> verifyResponse,
        IReadOnlyList<IHttpResponseExtractor<T>> extractors)
        : base(new HttpStepPreparer(requestBuilder), new HttpStepExecutor(),
            new HttpStepVerifier(verifyResponse), new HttpStepExtractor(extractors))
    {
    }

    private class HttpStepExecutor : IStepExecutor<T>
    {
        /// <inheritdoc />
        public async Task<ConfirmStatus> ExecuteStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            IHttpClientProvider httpClientProvider = stepContext.Services.GetRequiredService<IHttpClientProvider>();
            HttpClient httpClient = httpClientProvider.Provide();

            if (!stepContext.TryGetItem(out HttpRequestMessage? request) || request == null)
            {
                return ConfirmStatus.Failure;
            }

            HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            response.Content = new StringContent(content);

            stepContext.AddItem(response);

            return ConfirmStatus.Success;
        }
    }

    private class HttpStepExtractor : IStepExtractor<T>
    {
        public HttpStepExtractor(IReadOnlyList<IHttpResponseExtractor<T>> extractors)
        {
            Extractors = extractors;
        }

        private IReadOnlyList<IHttpResponseExtractor<T>> Extractors { get; }

        /// <inheritdoc />
        public async Task<ConfirmStatus> ExtractStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            if (Extractors.Count == 0)
            {
                return ConfirmStatus.Success;
            }

            if (!stepContext.TryGetItem(out HttpResponseMessage? response) || response == null)
            {
                return ConfirmStatus.Failure;
            }

            foreach (IHttpResponseExtractor<T> extractor in Extractors)
            {
                await extractor.Extract(stepContext, response, cancellationToken);
            }

            return ConfirmStatus.Success;
        }
    }

    private class HttpStepPreparer : IStepPreparer<T>
    {
        public HttpStepPreparer(RequestBuilder requestBuilder)
        {
            RequestBuilder = requestBuilder;
        }

        private RequestBuilder RequestBuilder { get; }

        /// <inheritdoc />
        public Task<ConfirmStatus> PrepareStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            IHttpClientProvider httpClientProvider = stepContext.Services.GetRequiredService<IHttpClientProvider>();
            HttpClient httpClient = httpClientProvider.Provide();
            Uri? baseAddress = httpClient.BaseAddress;
            HttpRequestMessage request = ((IHttpRequestMessageConverter)RequestBuilder)
                .ToHttpRequestMessageConverter(baseAddress, stepContext.Vars);

            stepContext.AddItem(request);

            return Task.FromResult(ConfirmStatus.Success);
        }
    }

    private class HttpStepVerifier : IStepVerifier<T>
    {
        public HttpStepVerifier(Func<HttpResponseMessage, StepContext<T>, CancellationToken, Task> verifyResponse)
        {
            VerifyResponse = verifyResponse;
        }

        private Func<HttpResponseMessage, StepContext<T>, CancellationToken, Task> VerifyResponse { get; }

        /// <inheritdoc />
        public async Task<ConfirmStatus> VerifyStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            if (!stepContext.TryGetItem(out HttpResponseMessage? response) || response == null)
            {
                return ConfirmStatus.Failure;
            }

            await VerifyResponse(response, stepContext, cancellationToken);

            return ConfirmStatus.Success;
        }
    }
}