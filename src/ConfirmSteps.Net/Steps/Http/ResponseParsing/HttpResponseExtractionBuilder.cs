namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Linq.Expressions;
using ConfirmSteps.Steps.Http.Rest;

/// <summary>
/// Provides a builder for configuring data extraction from an HTTP response.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public sealed class HttpResponseExtractionBuilder<T> : IHttpResponseExtractorProvider<T>
    where T : class
{
    private List<IHttpResponseExtractor<T>> Extractors { get; } = new();

    /// <summary>
    /// Extracts data from the HTTP response message and sets it to a property of the scenario data.
    /// </summary>
    /// <param name="property">The property to set.</param>
    /// <param name="extractor">The extractor function.</param>
    /// <returns>The current <see cref="HttpResponseExtractionBuilder{T}"/> for fluent chaining.</returns>
    public HttpResponseExtractionBuilder<T> ToData(Expression<Func<T, object>> property,
        Func<HttpResponseMessage, object?> extractor)
    {
        Extractors.Add(new HttpResponseMessageExtractor<T>(property, extractor));

        return this;
    }

    /// <summary>
    /// Extracts data from the HTTP response JSON and sets it to a property of the scenario data.
    /// </summary>
    /// <param name="property">The property to set.</param>
    /// <param name="extractor">The extractor function.</param>
    /// <returns>The current <see cref="HttpResponseExtractionBuilder{T}"/> for fluent chaining.</returns>
    public HttpResponseExtractionBuilder<T> ToData(Expression<Func<T, object>> property,
        Func<HttpResponseJson, object?> extractor)
    {
        Extractors.Add(new HttpResponseJsonExtractor<T>(property, extractor));

        return this;
    }

    /// <summary>
    /// Extracts data from the HTTP response message and adds it as a scenario variable.
    /// </summary>
    /// <param name="key">The variable key.</param>
    /// <param name="extractor">The extractor function.</param>
    /// <returns>The current <see cref="HttpResponseExtractionBuilder{T}"/> for fluent chaining.</returns>
    public HttpResponseExtractionBuilder<T> ToVars(string key, Func<HttpResponseMessage, object?> extractor)
    {
        Extractors.Add(new HttpResponseMessageExtractor<T>(key, extractor));

        return this;
    }

    /// <summary>
    /// Extracts data from the HTTP response JSON and adds it as a scenario variable.
    /// </summary>
    /// <param name="key">The variable key.</param>
    /// <param name="extractor">The extractor function.</param>
    /// <returns>The current <see cref="HttpResponseExtractionBuilder{T}"/> for fluent chaining.</returns>
    public HttpResponseExtractionBuilder<T> ToVars(string key, Func<HttpResponseJson, object?> extractor)
    {
        Extractors.Add(new HttpResponseJsonExtractor<T>(key, extractor));

        return this;
    }

    IReadOnlyList<IHttpResponseExtractor<T>> IHttpResponseExtractorProvider<T>.Provide()
    {
        return Extractors;
    }
}
