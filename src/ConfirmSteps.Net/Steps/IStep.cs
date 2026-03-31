namespace ConfirmSteps.Steps;

/// <summary>
/// Defines a single step in a scenario.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public interface IStep<T>
  where T : class
{
  /// <summary>
  /// Gets the title of the step.
  /// </summary>
  string Title { get; }

  /// <summary>
  /// Executes the step within the given context.
  /// </summary>
  /// <param name="stepContext">The context for the step execution.</param>
  /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
  /// <returns>A <see cref="StepResult{T}"/> containing the results of the step execution.</returns>
  Task<StepResult<T>> ConfirmStep(StepContext<T> stepContext, CancellationToken cancellationToken);
}
