namespace ConfirmSteps.Steps.Http.ResponseVerification;

/// <summary>
/// Defines a verifier that checks the HTTP response against expected conditions or assertions.
/// Implementations of this interface can perform various verification tasks, such as validating status codes,
/// checking response content, or asserting specific conditions based on the scenario requirements.
/// </summary>
/// <typeparam name="T">The type of the scenario data context.</typeparam>
public interface IHttpResponseVerifier<T>
  where T : class
{
  /// <summary>
  /// Verifies the HTTP response. Implementations can perform any necessary verification logic, such as checking status codes, validating response content, or asserting specific conditions based on the scenario requirements.
  /// </summary>
  /// <param name="stepContext">The step context, which provides access to scenario data and services.</param>
  /// <param name="response">The HTTP response message to be verified.</param>
  /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
  /// <returns>A task representing the asynchronous verification operation.</returns>
  Task Verify(StepContext<T> stepContext, HttpResponseMessage response,
    CancellationToken cancellationToken);
}
