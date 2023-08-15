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

        return jsonElement.Value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => throw new ArgumentException()
        };
    }

    public static bool?[]? SelectBooleanArray(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        if (!jsonDocumentProvider.SelectJsonElements(jsonPath, null, out IList<JsonElement>? jsonElements))
        {
            return null;
        }

        return jsonElements?.Where(e => e is { ValueKind: JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null })
            .Select(e => e.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => (bool?)null,
                _ => throw new ArgumentException()
            }).ToArray();
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

    public static decimal?[]? SelectNumberArray(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        if (!jsonDocumentProvider.SelectJsonElements(jsonPath, null,
                out IList<JsonElement>? jsonElements))
        {
            return null;
        }

        return jsonElements?.Where(e => e is { ValueKind: JsonValueKind.Number or JsonValueKind.Null })
            .Select(e => e.ValueKind switch
            {
                JsonValueKind.Number => e.GetDecimal(),
                JsonValueKind.Null => (decimal?)null,
                _ => throw new ArgumentException()
            }).ToArray();
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

    public static string?[]? SelectStringArray(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        if (!jsonDocumentProvider.SelectJsonElements(jsonPath, JsonValueKind.String,
                out IList<JsonElement>? jsonElements))
        {
            return null;
        }

        return jsonElements?.Select(e => e.GetString()).ToArray();
    }

    private static bool SelectJsonElement(this IJsonDocumentProvider jsonDocumentProvider,
        string jsonPath, JsonValueKind? jsonValueKind, out JsonElement? jsonElement)
    {
        jsonElement = null;

        if (!jsonDocumentProvider.SelectJsonElements(jsonPath, jsonValueKind,
                out IList<JsonElement>? localJsonElements))
        {
            return false;
        }

        if (localJsonElements is not { Count: 1 })
        {
            return false;
        }

        jsonElement = localJsonElements[0];
        return true;
    }

    private static bool SelectJsonElements(this IJsonDocumentProvider jsonDocumentProvider,
        string jsonPath, JsonValueKind? jsonValueKind, out IList<JsonElement>? jsonElements)
    {
        jsonElements = null;

        if (jsonDocumentProvider.JsonDocument == null)
        {
            return false;
        }

        IList<JsonElement> localJsonElements =
            JsonSelector.Select(jsonDocumentProvider.JsonDocument.RootElement, jsonPath);

        if (localJsonElements.Count == 0)
        {
            return false;
        }

        if (jsonValueKind.HasValue && localJsonElements.Any(e => e.ValueKind != jsonValueKind.Value))
        {
            return false;
        }

        jsonElements = localJsonElements;
        return true;
    }
}