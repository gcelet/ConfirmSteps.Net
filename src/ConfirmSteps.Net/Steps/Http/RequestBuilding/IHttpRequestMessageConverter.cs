namespace ConfirmSteps.Steps.Http.RequestBuilding;

public interface IHttpRequestMessageConverter
{
    HttpRequestMessage ToHttpRequestMessageConverter(Uri? baseAddress, IReadOnlyDictionary<string, object> vars);
}