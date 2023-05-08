namespace ConfirmSteps;

public sealed class ConfirmStepResult<T>
    where T : class
{
    private readonly List<StepResult<T>> _stepResults = new();

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

    public T Data { get; }

    public Exception? Exception { get; }

    public ConfirmStatus Status { get; }

    public IReadOnlyCollection<StepResult<T>> StepResults => _stepResults.AsReadOnly();

    public IReadOnlyDictionary<string, object> Vars { get; }

    public StepResult<T> this[int index] => _stepResults[index];
}