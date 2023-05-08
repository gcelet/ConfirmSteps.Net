namespace ConfirmSteps.Steps.Http.ResponseParsing;

public interface IHttpResponseExtractorProvider<T>
    where T : class
{
    IReadOnlyList<IHttpResponseExtractor<T>> Provide();
}