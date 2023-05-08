namespace ConfirmSteps.Steps;

public interface IStep<T>
    where T : class
{
    Task<StepResult<T>> ConfirmStep(StepContext<T> stepContext, CancellationToken cancellationToken);
}