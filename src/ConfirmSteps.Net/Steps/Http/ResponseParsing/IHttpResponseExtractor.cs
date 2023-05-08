namespace ConfirmSteps.Steps.Http.ResponseParsing;

public interface IHttpResponseExtractor<T>
    where T : class
{
    Task Extract(StepContext<T> stepContext, HttpResponseMessage response, CancellationToken cancellationToken);
}