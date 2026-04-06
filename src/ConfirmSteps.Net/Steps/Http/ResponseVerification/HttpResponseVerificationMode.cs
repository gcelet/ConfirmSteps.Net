namespace ConfirmSteps.Steps.Http.ResponseVerification;

/// <summary>
/// Defines the mode of HTTP response verification, determining how the verifiers are applied to the HTTP response
/// and how failures are handled during the verification process.
/// </summary>
public enum HttpResponseVerificationMode
{
  /// <summary>
  /// Verifies the HTTP response and stops at the first failure encountered among the provided verifiers.
  /// </summary>
  StopOnFirstFailure = 1,

  /// <summary>
  /// Verifies the HTTP response using all provided verifiers, regardless of any failures.
  /// </summary>
  VerifyAll = 2
}
