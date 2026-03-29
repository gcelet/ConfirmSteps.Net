namespace ConfirmSteps.Steps.Http;

/// <summary>
/// Defines a provider for an <see cref="HttpClient"/> instance.
/// </summary>
public interface IHttpClientProvider
{
    /// <summary>
    /// Provides an <see cref="HttpClient"/> instance.
    /// </summary>
    /// <returns>The <see cref="HttpClient"/> instance.</returns>
    HttpClient Provide();
}
