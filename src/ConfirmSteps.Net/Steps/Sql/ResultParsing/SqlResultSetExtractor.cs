namespace ConfirmSteps.Steps.Sql.ResultParsing;

using System.Linq.Expressions;

using ConfirmSteps.Internal;

/// <summary>
/// A SQL result set extractor that extracts data from a <see cref="SqlResultSet"/>.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public sealed class SqlResultSetExtractor<T> : ISqlResultSetExtractor<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlResultSetExtractor{T}"/> class that sets a property.
    /// </summary>
    /// <param name="property">The property to set.</param>
    /// <param name="extractor">The extractor function.</param>
    public SqlResultSetExtractor(Expression<Func<T, object>> property,
        Func<SqlResultSet, object?> extractor)
    {
        Setter = (stepContext, value) => SetData(property, stepContext, value);
        Extractor = extractor;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlResultSetExtractor{T}"/> class that sets a variable.
    /// </summary>
    /// <param name="varsKey">The variable key.</param>
    /// <param name="extractor">The extractor function.</param>
    public SqlResultSetExtractor(string varsKey, Func<SqlResultSet, object?> extractor)
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

    private Func<SqlResultSet, object?> Extractor { get; }

    private Action<StepContext<T>, object> Setter { get; }

    /// <inheritdoc />
    public Task Extract(StepContext<T> stepContext, SqlResultSet resultSet,
        CancellationToken cancellationToken)
    {
        object? value = Extractor(resultSet);

        if (value != null)
        {
            Setter(stepContext, value);
        }

        return Task.CompletedTask;
    }
}
