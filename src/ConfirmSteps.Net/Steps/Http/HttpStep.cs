namespace ConfirmSteps.Steps.Http;

using ConfirmSteps.Steps.Http.RequestBuilding;
using ConfirmSteps.Steps.Http.ResponseParsing;
using ConfirmSteps.Steps.Http.ResponseVerification;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents a step that performs an HTTP request and verifies the response.
/// </summary>
/// <typeparam name="T">The type of the custom data context.</typeparam>
public sealed class HttpStep<T> : Step<T>
  where T : class
{
  /// <summary>
  /// Initializes a new instance of the <see cref="HttpStep{T}"/> class.
  /// </summary>
  /// <param name="title">The title of the step.</param>
  /// <param name="requestBuilder">The builder used to construct the HTTP request.</param>
  /// <param name="verifiers">A list of verifiers to validate the HTTP response.</param>
  /// <param name="verificationMode">
  /// The mode of HTTP response verification, determining how the verifiers are applied and how failures are handled.
  /// </param>
  /// <param name="extractors">A list of extractors to pull data from the HTTP response.</param>
  public HttpStep(string title, RequestBuilder requestBuilder,
    IReadOnlyList<IHttpResponseVerifier<T>> verifiers,
    HttpResponseVerificationMode verificationMode,
    IReadOnlyList<IHttpResponseExtractor<T>> extractors)
    : base(title, new HttpStepPreparer(requestBuilder), new HttpStepExecutor(),
      new HttpStepVerifier(verifiers, verificationMode), new HttpStepExtractor(extractors))
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

      await response.Content.LoadIntoBufferAsync();
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
    public HttpStepVerifier(IReadOnlyList<IHttpResponseVerifier<T>> verifiers, HttpResponseVerificationMode verificationMode)
    {
      Verifiers = verifiers;
      VerificationMode = verificationMode;
    }

    private HttpResponseVerificationMode VerificationMode { get; }

    private IReadOnlyList<IHttpResponseVerifier<T>> Verifiers { get; }

    /// <inheritdoc />
    public async Task<ConfirmStatus> VerifyStep(StepContext<T> stepContext, CancellationToken cancellationToken)
    {
      if (!stepContext.TryGetItem(out HttpResponseMessage? response) || response == null)
      {
        return ConfirmStatus.Failure;
      }

      ConfirmStatus confirmStatus = ConfirmStatus.Success;
      List<Exception> exceptions = new();

      foreach (IHttpResponseVerifier<T> verifier in Verifiers)
      {
        try
        {
          await verifier.Verify(stepContext, response, cancellationToken);
        }
        catch (Exception exception)
        {
          confirmStatus = ConfirmStatus.Failure;
          exceptions.Add(exception);
          if (VerificationMode == HttpResponseVerificationMode.StopOnFirstFailure)
          {
            break;
          }
        }
      }

      return exceptions.Count switch
      {
        1 => throw exceptions[0],
        > 1 => throw new AggregateException(exceptions),
        _ => confirmStatus
      };
    }
  }
}
