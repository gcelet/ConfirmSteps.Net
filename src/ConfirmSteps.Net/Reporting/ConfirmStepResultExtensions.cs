namespace ConfirmSteps.Reporting;

/// <summary>
/// Extension methods on <see cref="ConfirmStepResult{T}"/> for producing execution summaries.
/// </summary>
public static class ConfirmStepResultExtensions
{
    /// <summary>
    /// Returns a human-readable summary of the scenario execution result using the default text reporter.
    /// The summary can be passed as the message argument to any assertion library.
    /// </summary>
    /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
    /// <param name="result">The scenario execution result.</param>
    /// <param name="configure">Optional delegate to configure the summary output.</param>
    /// <returns>A formatted summary string.</returns>
    public static string GetExecutionSummary<T>(this ConfirmStepResult<T> result,
        Action<ScenarioExecutionSummaryOptions>? configure = null) where T : class
    {
        ScenarioExecutionSummaryReporter reporter = configure is null
            ? ScenarioExecutionSummaryReporter.Default
            : new ScenarioExecutionSummaryReporter(configure);

        return reporter.Report(result);
    }

    /// <summary>
    /// Returns a human-readable summary of the scenario execution result using a custom reporter.
    /// The summary can be passed as the message argument to any assertion library.
    /// </summary>
    /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
    /// <param name="result">The scenario execution result.</param>
    /// <param name="reporter">The reporter to use for formatting the summary.</param>
    /// <returns>A formatted summary string.</returns>
    public static string GetExecutionSummary<T>(this ConfirmStepResult<T> result,
        IScenarioExecutionSummaryReporter reporter) where T : class
    {
        return reporter.Report(result);
    }
}
