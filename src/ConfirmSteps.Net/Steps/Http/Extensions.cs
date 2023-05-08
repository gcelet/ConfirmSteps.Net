namespace ConfirmSteps.Steps.Http;

using Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddExternalHttpClient(this IServiceCollection services, HttpClient httpClient)
    {
        ExternalHttpClientProvider externalHttpClientProvider = new(httpClient);

        return services.AddSingleton<IHttpClientProvider>(externalHttpClientProvider);
    }
}