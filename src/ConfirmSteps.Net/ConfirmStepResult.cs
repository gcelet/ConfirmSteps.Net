namespace ConfirmSteps;

/// <summary>
/// Represents the result of a scenario execution.
/// </summary>
/// <typeparam name="T">The type of the data object being processed.</typeparam>
public sealed class ConfirmStepResult<T>
    where T : class
{
    private readonly List<StepResult<T>> _stepResults = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmStepResult{T}"/> class.
    /// </summary>
    /// <param name="status">The final status of the scenario.</param>
    /// <param name="stepResults">The results of individual steps.</param>
    /// <param name="data">The data object after execution.</param>
    /// <param name="vars">The variables collected during execution.</param>
    /// <param name="exception">The exception that caused the scenario to fail, if any.</param>
    public ConfirmStepResult(ConfirmStatus status, IEnumerable<StepResult<T>> stepResults,
        T data, IReadOnlyDictionary<string, object> vars,
        Exception? exception = null)
    {
        Status = status;
        _stepResults.AddRange(stepResults);
        Data = data;
        Vars = vars;
        Exception = exception;
    }

    /// <summary>
    /// Gets the data object after execution.
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// Gets the exception that caused the scenario to fail, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the final status of the scenario.
    /// </summary>
    public ConfirmStatus Status { get; }

    /// <summary>
    /// Gets the results of all steps in the scenario.
    /// </summary>
    public IReadOnlyCollection<StepResult<T>> StepResults => _stepResults.AsReadOnly();

    /// <summary>
    /// Gets the variables collected during scenario execution.
    /// </summary>
    public IReadOnlyDictionary<string, object> Vars { get; }

    /// <summary>
    /// Gets the step result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the step result to get.</param>
    /// <returns>The step result at the specified index.</returns>
    public StepResult<T> this[int index] => _stepResults[index];
}
