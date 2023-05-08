namespace ConfirmSteps.Steps;

public interface IStepPreparer<T>
    where T : class
{
    Task<ConfirmStatus> PrepareStep(StepContext<T> stepContext, CancellationToken cancellationToken);
}