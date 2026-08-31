namespace ConfirmSteps.Steps.Sql.ResultParsing;

/// <summary>
/// Defines a provider that returns a list of SQL result set extractors.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public interface ISqlResultSetExtractorProvider<T>
    where T : class
{
    /// <summary>
    /// Provides the list of extractors.
    /// </summary>
    /// <returns>A read-only list of extractors.</returns>
    IReadOnlyList<ISqlResultSetExtractor<T>> Provide();
}
