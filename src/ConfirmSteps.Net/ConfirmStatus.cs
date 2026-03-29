namespace ConfirmSteps;

/// <summary>
/// Defines the status of a step or a scenario execution.
/// </summary>
public enum ConfirmStatus
{
    /// <summary>
    /// The step or scenario completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The step or scenario failed.
    /// </summary>
    Failure,

    /// <summary>
    /// The step or scenario status is indecisive (e.g. skipped due to previous failure).
    /// </summary>
    Indecisive
}
