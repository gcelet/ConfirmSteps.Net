namespace ConfirmSteps.Steps;

/// <summary>
/// Provides a mechanism to execute the core logic of a step.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public interface IStepExecutor<T>
    where T : class
{
    /// <summary>
    /// Executes the core logic of the step.
    /// </summary>
    /// <param name="stepContext">The context for the step execution.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>The <see cref="ConfirmStatus"/> resulting from the execution.</returns>
    Task<ConfirmStatus> ExecuteStep(StepContext<T> stepContext, CancellationToken cancellationToken);
}
