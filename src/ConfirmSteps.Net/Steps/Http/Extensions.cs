namespace ConfirmSteps.Steps.Http;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for HTTP steps configuration.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers an external <see cref="HttpClient"/> to be used by the HTTP steps.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the provider to.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
    /// <returns>The <see cref="IServiceCollection"/> for further configuration.</returns>
    public static IServiceCollection AddExternalHttpClient(this IServiceCollection services, HttpClient httpClient)
    {
        ExternalHttpClientProvider externalHttpClientProvider = new(httpClient);

        return services.AddSingleton<IHttpClientProvider>(externalHttpClientProvider);
    }
}
