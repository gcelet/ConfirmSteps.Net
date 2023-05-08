namespace ConfirmSteps.Steps;

public interface IStepVerifier<T>
    where T : class
{
    Task<ConfirmStatus> VerifyStep(StepContext<T> stepContext, CancellationToken cancellationToken);
}