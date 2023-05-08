namespace ConfirmSteps.Steps;

public interface IStepExtractor<T>
    where T : class
{
    Task<ConfirmStatus> ExtractStep(StepContext<T> stepContext, CancellationToken cancellationToken);
}