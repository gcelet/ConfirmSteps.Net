namespace ConfirmSteps;

using ConfirmSteps.Steps;

/// <summary>
/// Observes the progress of a scenario as it runs.
/// </summary>
/// <remarks>
/// <para>
/// Implementations are discovered through dependency injection: register one or several with
/// <see cref="IScenarioCustomizer{T}.WithServices"/> and <see cref="Scenario{T}.ConfirmSteps"/>
/// will call them. Nothing else changes — the signature of <c>ConfirmSteps</c> is untouched and a
/// scenario without observers pays nothing.
/// </para>
/// <para>
/// Observing must never change the outcome being observed: an exception thrown by an observer is
/// swallowed rather than allowed to fail the step it was reporting on.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public interface IScenarioObserver<T>
    where T : class
{
    /// <summary>
    /// Called once before the first step runs.
    /// </summary>
    /// <param name="scenarioContext">The context of the scenario about to run.</param>
    /// <param name="stepCount">The number of steps the scenario will run.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the observer is done.</returns>
    ValueTask OnScenarioStarting(ScenarioContext<T> scenarioContext, int stepCount,
        CancellationToken cancellationToken);

    /// <summary>
    /// Called before each step, including steps skipped after an earlier failure.
    /// </summary>
    /// <param name="scenarioContext">The context of the running scenario.</param>
    /// <param name="step">The step about to run.</param>
    /// <param name="stepIndex">The zero-based index of the step.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the observer is done.</returns>
    ValueTask OnStepStarting(ScenarioContext<T> scenarioContext, IStep<T> step, int stepIndex,
        CancellationToken cancellationToken);

    /// <summary>
    /// Called after each step, with its result.
    /// </summary>
    /// <param name="scenarioContext">The context of the running scenario.</param>
    /// <param name="stepResult">The result of the step that just ran.</param>
    /// <param name="stepIndex">The zero-based index of the step.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the observer is done.</returns>
    ValueTask OnStepCompleted(ScenarioContext<T> scenarioContext, StepResult<T> stepResult, int stepIndex,
        CancellationToken cancellationToken);

    /// <summary>
    /// Called once after the last step, with the scenario result.
    /// </summary>
    /// <param name="confirmStepResult">The result of the scenario.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the observer is done.</returns>
    ValueTask OnScenarioCompleted(ConfirmStepResult<T> confirmStepResult,
        CancellationToken cancellationToken);
}
