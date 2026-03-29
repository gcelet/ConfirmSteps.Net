namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Linq.Expressions;
using ConfirmSteps.Internal;

/// <summary>
/// An HTTP response extractor that extracts data directly from the <see cref="HttpResponseMessage"/>.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public sealed class HttpResponseMessageExtractor<T> : IHttpResponseExtractor<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseMessageExtractor{T}"/> class that sets a property.
    /// </summary>
    /// <param name="property">The property to set.</param>
    /// <param name="extractor">The extractor function.</param>
    public HttpResponseMessageExtractor(Expression<Func<T, object>> property,
        Func<HttpResponseMessage, object?> extractor)
    {
        Setter = (stepContext, value) => SetData(property, stepContext, value);
        Extractor = extractor;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseMessageExtractor{T}"/> class that sets a variable.
    /// </summary>
    /// <param name="varsKey">The variable key.</param>
    /// <param name="extractor">The extractor function.</param>
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
