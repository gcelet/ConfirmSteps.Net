namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Linq.Expressions;
using ConfirmSteps.Internal;
using ConfirmSteps.Steps.Http.Rest;

public sealed class HttpResponseJsonExtractor<T> : IHttpResponseExtractor<T>
    where T : class
{
    public HttpResponseJsonExtractor(Expression<Func<T, object>> property, Func<HttpResponseJson, object?> extractor)
    {
        Setter = (stepContext, value) => SetData(property, stepContext, value);
        Extractor = extractor;
    }

    public HttpResponseJsonExtractor(string varsKey, Func<HttpResponseJson, object?> extractor)
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

    private Func<HttpResponseJson, object?> Extractor { get; }

    private Action<StepContext<T>, object> Setter { get; }

    /// <inheritdoc />
    public async Task Extract(StepContext<T> stepContext, HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        HttpResponseJson jsonResponse = await stepContext.ParseJson(response, cancellationToken);
        object? value = Extractor(jsonResponse);

        if (value != null)
        {
            Setter(stepContext, value);
        }
    }
}