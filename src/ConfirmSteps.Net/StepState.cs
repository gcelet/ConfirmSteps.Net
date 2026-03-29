namespace ConfirmSteps;

/// <summary>
/// Defines the execution states of a step.
/// </summary>
public enum StepState
{
    /// <summary>
    /// The step is waiting to be executed.
    /// </summary>
    Idle,

    /// <summary>
    /// The step is in the preparation phase.
    /// </summary>
    Prepare,

    /// <summary>
    /// The step is in the execution phase.
    /// </summary>
    Execute,

    /// <summary>
    /// The step is in the verification phase.
    /// </summary>
    Verify,

    /// <summary>
    /// The step is in the data extraction phase.
    /// </summary>
    Extract,

    /// <summary>
    /// The step has completed all phases.
    /// </summary>
    Done
}
