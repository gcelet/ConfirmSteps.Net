namespace ConfirmSteps.Steps.Sql.ResultParsing;

using System.Linq.Expressions;

/// <summary>
/// Provides a builder for configuring data extraction from a <see cref="SqlResultSet"/>.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public sealed class SqlResultSetExtractionBuilder<T> : ISqlResultSetExtractorProvider<T>
    where T : class
{
    private List<ISqlResultSetExtractor<T>> Extractors { get; } = new();

    /// <summary>
    /// Configures extraction of a value from the result set into a scenario data property.
    /// </summary>
    /// <param name="property">The property to set.</param>
    /// <param name="extractor">A function that extracts the value from the <see cref="SqlResultSet"/>.</param>
    /// <returns>The current <see cref="SqlResultSetExtractionBuilder{T}"/> for fluent chaining.</returns>
    public SqlResultSetExtractionBuilder<T> ToData(Expression<Func<T, object>> property,
        Func<SqlResultSet, object?> extractor)
    {
        Extractors.Add(new SqlResultSetExtractor<T>(property, extractor));

        return this;
    }

    /// <summary>
    /// Configures extraction of a value from the result set into a scenario variable.
    /// </summary>
    /// <param name="key">The variable key.</param>
    /// <param name="extractor">A function that extracts the value from the <see cref="SqlResultSet"/>.</param>
    /// <returns>The current <see cref="SqlResultSetExtractionBuilder{T}"/> for fluent chaining.</returns>
    public SqlResultSetExtractionBuilder<T> ToVars(string key, Func<SqlResultSet, object?> extractor)
    {
        Extractors.Add(new SqlResultSetExtractor<T>(key, extractor));

        return this;
    }

    /// <inheritdoc />
    IReadOnlyList<ISqlResultSetExtractor<T>> ISqlResultSetExtractorProvider<T>.Provide()
    {
        return Extractors;
    }
}
