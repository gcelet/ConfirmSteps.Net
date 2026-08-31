namespace ConfirmSteps.Steps;

/// <summary>
/// Defines the mode of step verification, determining how the verifiers are applied to the step's result
/// and how failures are handled during the verification process.
/// </summary>
public enum StepVerificationMode
{
  /// <summary>
  /// Verifies the step's result and stops at the first failure encountered among the provided verifiers.
  /// </summary>
  StopOnFirstFailure = 1,

  /// <summary>
  /// Verifies the step's result using all provided verifiers, regardless of any failures.
  /// </summary>
  VerifyAll = 2
}
