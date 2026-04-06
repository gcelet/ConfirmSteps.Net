namespace ConfirmSteps.Steps.Http.ResponseVerification;

/// <summary>
/// A simple implementation of <see cref="IHttpResponseVerifier{T}"/> that allows verification logic to be defined through a provided function.
/// This class can be used for straightforward verification scenarios where the verification logic can be encapsulated
/// in a single function, providing flexibility and ease of use for common verification tasks.
/// </summary>
/// <typeparam name="T">The type of the scenario data context.</typeparam>
public class HttpResponseMessageVerifier<T> : IHttpResponseVerifier<T>
  where T : class
{
  /// <summary>
  /// Initializes a new instance of the <see cref="HttpResponseMessageVerifier{T}"/> class.
  /// </summary>
  /// <param name="verifyFunc">
  /// A function that defines the verification logic to be applied directly to the <see cref="HttpResponseMessage"/> object.
  /// </param>
  public HttpResponseMessageVerifier(Func<HttpResponseMessage, StepContext<T>, CancellationToken, Task> verifyFunc)
  {
    VerifyFunc = verifyFunc;
  }

  private Func<HttpResponseMessage, StepContext<T>, CancellationToken, Task> VerifyFunc { get; }

  /// <inheritdoc />
  public async Task Verify(StepContext<T> stepContext, HttpResponseMessage response,
    CancellationToken cancellationToken)
  {
    await VerifyFunc(response, stepContext, cancellationToken);
  }
}
