namespace ConfirmSteps.Reporting;

/// <summary>
/// Defines a reporter that produces an execution summary string for a <see cref="ConfirmStepResult{T}"/>.
/// </summary>
public interface IScenarioExecutionSummaryReporter
{
  /// <summary>
  /// Produces a human-readable summary of the scenario execution result.
  /// </summary>
  /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
  /// <param name="result">The scenario execution result.</param>
  /// <returns>A formatted summary string.</returns>
  string Report<T>(ConfirmStepResult<T> result) where T : class;
}
