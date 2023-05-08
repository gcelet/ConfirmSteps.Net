namespace ConfirmSteps.Steps;

public interface IStepExecutor<T>
    where T : class
{
    Task<ConfirmStatus> ExecuteStep(StepContext<T> stepContext, CancellationToken cancellationToken);
}