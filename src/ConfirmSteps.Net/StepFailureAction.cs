namespace ConfirmSteps;

/// <summary>
/// What a scenario does with the steps that follow one which did not succeed.
/// </summary>
/// <remarks>
/// Neither value changes the outcome of the scenario. A step that fails fails the scenario, and no
/// step running afterwards can turn that verdict back into a success — the names below say what is
/// actually being chosen: whether the remaining steps are <b>observed</b>, not whether the failure
/// counts.
/// </remarks>
public enum StepFailureAction
{
    /// <summary>
    /// Leave the remaining steps unexecuted, reported as <see cref="ConfirmStatus.Indecisive"/>.
    /// </summary>
    /// <remarks>
    /// The behaviour a scenario has always had, and the one that applies when no
    /// <see cref="IStepFailurePolicy{T}"/> is registered. Once a step has failed, the rest of the
    /// journey proves nothing.
    /// </remarks>
    SkipRemainingSteps = 0,

    /// <summary>
    /// Keep executing the remaining steps, purely to observe them. The scenario still fails.
    /// </summary>
    /// <remarks>
    /// For a host that measures rather than asserts. When a run stops at the first failure, every
    /// later step loses a sample precisely when errors appear, and per-step counters become unequal
    /// at the moment they most need to be comparable.
    /// </remarks>
    ContinueForObservation,
}
