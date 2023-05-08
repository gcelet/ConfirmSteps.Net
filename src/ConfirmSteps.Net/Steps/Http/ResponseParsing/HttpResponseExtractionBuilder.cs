namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Linq.Expressions;
using ConfirmSteps.Steps.Http.Rest;

public sealed class HttpResponseExtractionBuilder<T> : IHttpResponseExtractorProvider<T>
    where T : class
{
    private List<IHttpResponseExtractor<T>> Extractors { get; } = new();

    public HttpResponseExtractionBuilder<T> ToData(Expression<Func<T, object>> property,
        Func<HttpResponseMessage, object?> extractor)
    {
        Extractors.Add(new HttpResponseMessageExtractor<T>(property, extractor));

        return this;
    }

    public HttpResponseExtractionBuilder<T> ToData(Expression<Func<T, object>> property,
        Func<HttpResponseJson, object?> extractor)
    {
        Extractors.Add(new HttpResponseJsonExtractor<T>(property, extractor));

        return this;
    }

    public HttpResponseExtractionBuilder<T> ToVars(string key, Func<HttpResponseMessage, object?> extractor)
    {
        Extractors.Add(new HttpResponseMessageExtractor<T>(key, extractor));

        return this;
    }

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