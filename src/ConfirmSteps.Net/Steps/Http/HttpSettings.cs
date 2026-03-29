namespace ConfirmSteps.Steps.Http;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides global configuration settings for HTTP operations.
/// </summary>
public static class HttpSettings
{
    /// <summary>
    /// Gets or sets the delegate that builds the <see cref="JsonSerializerOptions"/> used for HTTP JSON operations.
    /// </summary>
    public static Func<JsonSerializerOptions> BuildJsonSerializerOptions { get; set; } = DefaultJsonSerializerOptions;

    private static JsonSerializerOptions DefaultJsonSerializerOptions()
    {
        JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(allowIntegerValues: false)
            },
        };

        return jsonSerializerOptions;
    }
}
