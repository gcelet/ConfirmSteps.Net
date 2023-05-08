namespace ConfirmSteps.Steps.Http;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class HttpSettings
{
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