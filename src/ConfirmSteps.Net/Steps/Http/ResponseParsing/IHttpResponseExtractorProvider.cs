namespace ConfirmSteps.Steps.Http.ResponseParsing;

/// <summary>
/// Defines a provider that returns a list of HTTP response extractors.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public interface IHttpResponseExtractorProvider<T>
    where T : class
{
    /// <summary>
    /// Provides the list of extractors.
    /// </summary>
    /// <returns>A read-only list of extractors.</returns>
    IReadOnlyList<IHttpResponseExtractor<T>> Provide();
}
