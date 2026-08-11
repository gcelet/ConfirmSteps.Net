namespace ConfirmSteps;

using ConfirmSteps.Steps;

/// <summary>
/// Decides what a scenario does with the steps that follow one which did not succeed.
/// </summary>
/// <remarks>
/// <para>
/// The rule this extension point does <b>not</b> touch: a step that does not succeed makes the
/// scenario fail, and the first such step decides the outcome. Nothing a policy can return changes
/// that, and a step running after the failure cannot turn the verdict back into a success. What is
/// delegated is narrower — whether the remaining steps are still <b>executed</b>, so a host can
/// observe them.
/// </para>
/// <para>
/// Discovered through dependency injection, like <see cref="IScenarioObserver{T}"/>: register one
/// with <see cref="IScenarioCustomizer{T}.WithServices"/> and
/// <see cref="Scenario{T}.ConfirmSteps(T, CancellationToken)"/> will consult it. Registering
/// nothing keeps the historical behaviour, the signature of <c>ConfirmSteps</c> is untouched, and a
/// scenario without a policy pays a single container lookup. Where several are registered, the last
/// one wins, as dependency injection resolution dictates.
/// </para>
/// <para>
/// A functional test wants the default; a load harness wants
/// <see cref="StepFailureAction.ContinueForObservation"/>, because a run that stops at the first
/// failure makes every later step lose a sample exactly when errors appear.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public interface IStepFailurePolicy<T>
    where T : class
{
    /// <summary>
    /// Called after a step returned anything other than <see cref="ConfirmStatus.Success"/>.
    /// </summary>
    /// <remarks>
    /// Consulted on <b>any</b> non-success outcome, which is what has always ended a run — not on
    /// <see cref="ConfirmStatus.Failure"/> alone. It is called once, on the step that decided the
    /// scenario's outcome; returning <see cref="StepFailureAction.ContinueForObservation"/> does not
    /// cause it to be asked again about later failures, since the verdict is already settled.
    /// </remarks>
    /// <param name="scenarioContext">The context of the running scenario.</param>
    /// <param name="stepResult">The result of the step that did not succeed.</param>
    /// <param name="stepIndex">Zero-based index of that step in the scenario.</param>
    /// <returns>Whether the remaining steps are still executed.</returns>
    StepFailureAction OnStepFailed(
        ScenarioContext<T> scenarioContext, StepResult<T> stepResult, int stepIndex);
}
