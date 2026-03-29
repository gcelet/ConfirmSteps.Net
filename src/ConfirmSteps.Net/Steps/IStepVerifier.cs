namespace ConfirmSteps.Steps;

/// <summary>
/// Provides a mechanism to verify the results of a step's execution.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public interface IStepVerifier<T>
    where T : class
{
    /// <summary>
    /// Verifies the step's execution results.
    /// </summary>
    /// <param name="stepContext">The context for the step execution.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>The <see cref="ConfirmStatus"/> resulting from the verification.</returns>
    Task<ConfirmStatus> VerifyStep(StepContext<T> stepContext, CancellationToken cancellationToken);
}
