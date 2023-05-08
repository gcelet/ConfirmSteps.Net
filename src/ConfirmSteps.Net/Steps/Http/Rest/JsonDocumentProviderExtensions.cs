namespace ConfirmSteps.Steps.Http.Rest;

using System.Text.Json;
using JsonCons.JsonPath;

public static class JsonDocumentProviderExtensions
{
    public static bool? SelectBoolean(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        if (!jsonDocumentProvider.SelectJsonElement(jsonPath, null, out JsonElement? jsonElement) ||
            !jsonElement.HasValue)
        {
            return null;
        }

        switch (jsonElement.Value.ValueKind)
        {
            case JsonValueKind.True:
            {
                return true;
            }
            case JsonValueKind.False:
            {
                return false;
            }
            default:
            {
                return null;
            }
        }
    }

    public static decimal? SelectNumber(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        if (!jsonDocumentProvider.SelectJsonElement(jsonPath, JsonValueKind.Number, out JsonElement? jsonElement) ||
            !jsonElement.HasValue)
        {
            return null;
        }

        return jsonElement.Value.GetDecimal();
    }

    public static string? SelectString(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        if (!jsonDocumentProvider.SelectJsonElement(jsonPath, JsonValueKind.String, out JsonElement? jsonElement) ||
            !jsonElement.HasValue)
        {
            return null;
        }

        return jsonElement.Value.GetString();
    }

    private static bool SelectJsonElement(this IJsonDocumentProvider jsonDocumentProvider,
        string jsonPath, JsonValueKind? jsonValueKind, out JsonElement? jsonElement)
    {
        jsonElement = null;

        if (jsonDocumentProvider.JsonDocument == null)
        {
            return false;
        }

        IList<JsonElement> jsonElements = JsonSelector.Select(jsonDocumentProvider.JsonDocument.RootElement, jsonPath);

        if (jsonElements.Count != 1)
        {
            return false;
        }

        JsonElement localJsonElement = jsonElements[0];

        if (jsonValueKind.HasValue && localJsonElement.ValueKind != jsonValueKind.Value)
        {
            return false;
        }

        jsonElement = localJsonElement;
        return true;
    }
}