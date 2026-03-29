namespace ConfirmSteps.Steps;

/// <summary>
/// Provides a mechanism to prepare for a step's execution.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public interface IStepPreparer<T>
    where T : class
{
    /// <summary>
    /// Prepares the step for execution.
    /// </summary>
    /// <param name="stepContext">The context for the step execution.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>The <see cref="ConfirmStatus"/> resulting from the preparation.</returns>
    Task<ConfirmStatus> PrepareStep(StepContext<T> stepContext, CancellationToken cancellationToken);
}
