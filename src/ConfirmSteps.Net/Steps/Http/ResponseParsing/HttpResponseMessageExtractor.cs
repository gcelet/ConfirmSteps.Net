namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Linq.Expressions;
using ConfirmSteps.Internal;

public sealed class HttpResponseMessageExtractor<T> : IHttpResponseExtractor<T>
    where T : class
{
    public HttpResponseMessageExtractor(Expression<Func<T, object>> property,
        Func<HttpResponseMessage, object?> extractor)
    {
        Setter = (stepContext, value) => SetData(property, stepContext, value);
        Extractor = extractor;
    }

    public HttpResponseMessageExtractor(string varsKey, Func<HttpResponseMessage, object?> extractor)
    {
        Setter = (stepContext, value) => SetVars(varsKey, stepContext, value);
        Extractor = extractor;
    }

    private static void SetData(Expression<Func<T, object>> property, StepContext<T> stepContext, object value)
    {
        T data = stepContext.ScenarioContext.Data;
        ReflectionHelper.SetProperty(property, data, value);
    }

    private static void SetVars(string varsKey, StepContext<T> stepContext, object value)
    {
        stepContext.Vars[varsKey] = value;
    }

    private Func<HttpResponseMessage, object?> Extractor { get; }

    private Action<StepContext<T>, object> Setter { get; }

    /// <inheritdoc />
    public Task Extract(StepContext<T> stepContext, HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        object? value = Extractor(response);

        if (value != null)
        {
            Setter(stepContext, value);
        }

        return Task.CompletedTask;
    }
}