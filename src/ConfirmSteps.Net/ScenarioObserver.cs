namespace ConfirmSteps;

using ConfirmSteps.Steps;

/// <summary>
/// A no-op <see cref="IScenarioObserver{T}"/> to derive from, so an implementation only overrides
/// the callbacks it cares about.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public abstract class ScenarioObserver<T> : IScenarioObserver<T>
    where T : class
{
    /// <inheritdoc />
    public virtual ValueTask OnScenarioStarting(ScenarioContext<T> scenarioContext, int stepCount,
        CancellationToken cancellationToken)
    {
        return default;
    }

    /// <inheritdoc />
    public virtual ValueTask OnStepStarting(ScenarioContext<T> scenarioContext, IStep<T> step, int stepIndex,
        CancellationToken cancellationToken)
    {
        return default;
    }

    /// <inheritdoc />
    public virtual ValueTask OnStepCompleted(ScenarioContext<T> scenarioContext, StepResult<T> stepResult,
        int stepIndex, CancellationToken cancellationToken)
    {
        return default;
    }

    /// <inheritdoc />
    public virtual ValueTask OnScenarioCompleted(ConfirmStepResult<T> confirmStepResult,
        CancellationToken cancellationToken)
    {
        return default;
    }
}
