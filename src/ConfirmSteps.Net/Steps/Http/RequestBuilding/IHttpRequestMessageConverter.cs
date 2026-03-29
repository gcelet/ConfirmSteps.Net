namespace ConfirmSteps.Steps.Http.RequestBuilding;

/// <summary>
/// Defines a converter that creates an <see cref="HttpRequestMessage"/>.
/// </summary>
public interface IHttpRequestMessageConverter
{
    /// <summary>
    /// Converts the configured request to an <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <param name="baseAddress">The base address for the request.</param>
    /// <param name="vars">The variables to use for template replacement.</param>
    /// <returns>The created <see cref="HttpRequestMessage"/>.</returns>
    HttpRequestMessage ToHttpRequestMessageConverter(Uri? baseAddress, IReadOnlyDictionary<string, object> vars);
}
