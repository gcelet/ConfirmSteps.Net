namespace ConfirmSteps;

/// <summary>
/// Represents the result of a single step execution.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public sealed class StepResult<T>
    where T : class
{
    /// <summary>
    /// Gets or sets the exception that occurred during step execution, if any.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the state of the step.
    /// </summary>
    public StepState State { get; set; } = StepState.Idle;

    /// <summary>
    /// Gets or sets the status of the step.
    /// </summary>
    public ConfirmStatus Status { get; set; } = ConfirmStatus.Indecisive;

    /// <summary>
    /// Gets or sets the variables collected or modified during step execution.
    /// </summary>
    public IReadOnlyDictionary<string, object> Vars { get; set; } = new Dictionary<string, object>();
}
