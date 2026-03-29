namespace ConfirmSteps.Steps;

/// <summary>
/// Provides a mechanism to extract data or variables after a step's execution.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public interface IStepExtractor<T>
    where T : class
{
    /// <summary>
    /// Extracts data or variables from the current step's execution results.
    /// </summary>
    /// <param name="stepContext">The context for the step execution.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>The <see cref="ConfirmStatus"/> resulting from the extraction.</returns>
    Task<ConfirmStatus> ExtractStep(StepContext<T> stepContext, CancellationToken cancellationToken);
}
