namespace ConfirmSteps;

public sealed class StepResult<T>
    where T : class
{
    public Exception? Exception { get; set; }

    public StepState State { get; set; } = StepState.Idle;

    public ConfirmStatus Status { get; set; } = ConfirmStatus.Indecisive;

    public IReadOnlyDictionary<string, object> Vars { get; set; } = new Dictionary<string, object>();
}