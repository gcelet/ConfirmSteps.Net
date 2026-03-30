namespace ConfirmSteps.Steps.Http.Rest;

using System.Text.Json;

using JsonCons.JmesPath;

using JsonCons.JsonPath;

/// <summary>
/// Provides extension methods for <see cref="IJsonDocumentProvider"/> to query JSON data using JSON path or JMESPath.
/// </summary>
public static class JsonDocumentProviderExtensions
{
    /// <summary>
    /// Selects a boolean value from the JSON document using a JSON path.
    /// </summary>
    /// <param name="jsonDocumentProvider">The JSON document provider.</param>
    /// <param name="jsonPath">The JSON path.</param>
    /// <returns>The selected boolean value, or null if not found or not a boolean.</returns>
    public static bool? SelectBoolean(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        (bool found, JsonElement? jsonElement) = jsonDocumentProvider.SelectJsonElement(jsonPath, null);

        if (!found || !jsonElement.HasValue)
        {
          return null;
        }

        return jsonElement.Value.ValueKind switch
        {
          JsonValueKind.True => true,
          JsonValueKind.False => false,
          JsonValueKind.Null => null,
          _ => throw new ArgumentException(),
        };
    }

    /// <summary>
    /// Selects an array of boolean values from the JSON document using a JSON path.
    /// </summary>
    /// <param name="jsonDocumentProvider">The JSON document provider.</param>
    /// <param name="jsonPath">The JSON path.</param>
    /// <returns>An array of boolean values, or null if not found.</returns>
    public static bool?[]? SelectBooleanArray(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        (bool found, IList<JsonElement>? jsonElements) = jsonDocumentProvider.SelectJsonElements(jsonPath, null);

        if (!found)
        {
          return null;
        }

        return jsonElements?.Where(e => e is
            { ValueKind: JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null })
          .Select(e => e.ValueKind switch
          {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => (bool?)null,
            _ => throw new ArgumentException(),
          }).ToArray();
    }

    /// <summary>
    /// Selects a numeric value from the JSON document using a JSON path.
    /// </summary>
    /// <param name="jsonDocumentProvider">The JSON document provider.</param>
    /// <param name="jsonPath">The JSON path.</param>
    /// <returns>The selected decimal value, or null if not found or not a number.</returns>
    public static decimal? SelectNumber(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        (bool found, JsonElement? jsonElement) = jsonDocumentProvider.SelectJsonElement(jsonPath, JsonValueKind.Number);

        if (!found || !jsonElement.HasValue)
        {
          return null;
        }

        return jsonElement.Value.GetDecimal();
    }

    /// <summary>
    /// Selects an array of numeric values from the JSON document using a JSON path.
    /// </summary>
    /// <param name="jsonDocumentProvider">The JSON document provider.</param>
    /// <param name="jsonPath">The JSON path.</param>
    /// <returns>An array of decimal values, or null if not found.</returns>
    public static decimal?[]? SelectNumberArray(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        (bool found, IList<JsonElement>? jsonElements) = jsonDocumentProvider.SelectJsonElements(jsonPath, null);

        if (!found)
        {
          return null;
        }

        return jsonElements?.Where(e => e is { ValueKind: JsonValueKind.Number or JsonValueKind.Null })
          .Select(e => e.ValueKind switch
          {
            JsonValueKind.Number => e.GetDecimal(),
            JsonValueKind.Null => (decimal?)null,
            _ => throw new ArgumentException(),
          }).ToArray();
    }

    /// <summary>
    /// Selects a string value from the JSON document using a JSON path.
    /// </summary>
    /// <param name="jsonDocumentProvider">The JSON document provider.</param>
    /// <param name="jsonPath">The JSON path.</param>
    /// <returns>The selected string value, or null if not found or not a string.</returns>
    public static string? SelectString(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        (bool found, JsonElement? jsonElement) = jsonDocumentProvider.SelectJsonElement(jsonPath, JsonValueKind.String);
        if (!found || !jsonElement.HasValue)
        {
          return null;
        }

        return jsonElement.Value.GetString();
    }

    /// <summary>
    /// Selects an array of string values from the JSON document using a JSON path.
    /// </summary>
    /// <param name="jsonDocumentProvider">The JSON document provider.</param>
    /// <param name="jsonPath">The JSON path.</param>
    /// <returns>An array of string values, or null if not found.</returns>
    public static string?[]? SelectStringArray(this IJsonDocumentProvider jsonDocumentProvider, string jsonPath)
    {
        (bool found, IList<JsonElement>? jsonElements) = jsonDocumentProvider.SelectJsonElements(jsonPath, JsonValueKind.String);

        if (!found)
        {
          return null;
        }

        return jsonElements?.Select(e => e.GetString()).ToArray();
    }

    /// <summary>
    /// Transforms the JSON data using JMESPath and deserializes it into an anonymous object type.
    /// </summary>
    /// <typeparam name="T">The type of the anonymous object.</typeparam>
    /// <param name="jsonDocumentProvider">The JSON document provider.</param>
    /// <param name="jmesPath">The JMESPath expression.</param>
    /// <param name="anonymousObjectInstance">An instance of the anonymous object to determine the type.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>The deserialized object, or null if transformation failed.</returns>
    public static T? TransformToAnonymousObject<T>(this IJsonDocumentProvider jsonDocumentProvider,
        string jmesPath, T anonymousObjectInstance,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return TransformToObject<T>(jsonDocumentProvider, jmesPath, jsonSerializerOptions);
    }

    /// <summary>
    /// Transforms the JSON data using JMESPath and deserializes it into an array of anonymous objects.
    /// </summary>
    /// <typeparam name="T">The type of the anonymous object.</typeparam>
    /// <param name="jsonDocumentProvider">The JSON document provider.</param>
    /// <param name="jmesPath">The JMESPath expression.</param>
    /// <param name="anonymousObjectInstance">An instance of the anonymous object to determine the type.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>An array of deserialized objects, or null if transformation failed.</returns>
    public static T[]? TransformToAnonymousObjectArray<T>(this IJsonDocumentProvider jsonDocumentProvider,
        string jmesPath, T anonymousObjectInstance,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return TransformToObjectArray<T>(jsonDocumentProvider, jmesPath, jsonSerializerOptions);
    }

    /// <summary>
    /// Transforms the JSON data using JMESPath and deserializes it into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="jsonDocumentProvider">The JSON document provider.</param>
    /// <param name="jmesPath">The JMESPath expression.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>The deserialized object, or default value of T if transformation failed.</returns>
    public static T? TransformToObject<T>(this IJsonDocumentProvider jsonDocumentProvider, string jmesPath,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        if (jsonDocumentProvider.JsonDocument?.RootElement == null)
        {
            return default;
        }

        JsonDocument? jsonDocument =
            JsonTransformer.Transform(jsonDocumentProvider.JsonDocument.RootElement, jmesPath);

        if (jsonDocument == null)
        {
            return default;
        }

        jsonSerializerOptions ??= HttpSettings.BuildJsonSerializerOptions();
        T? result = jsonDocument.Deserialize<T>(jsonSerializerOptions);

        return result;
    }

    /// <summary>
    /// Transforms the JSON data using JMESPath and deserializes it into an array of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="jsonDocumentProvider">The JSON document provider.</param>
    /// <param name="jmesPath">The JMESPath expression.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    /// <returns>An array of deserialized objects, or default value of T[] if transformation failed.</returns>
    public static T[]? TransformToObjectArray<T>(this IJsonDocumentProvider jsonDocumentProvider, string jmesPath,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        if (jsonDocumentProvider.JsonDocument?.RootElement == null)
        {
            return default;
        }

        JsonTransformer jsonTransformer = JsonTransformer.Parse(jmesPath);
        JsonDocument? jsonDocument =
            jsonTransformer.Transform(jsonDocumentProvider.JsonDocument.RootElement);

        if (jsonDocument == null)
        {
            return default;
        }

        jsonSerializerOptions ??= HttpSettings.BuildJsonSerializerOptions();
        T[]? result = jsonDocument.Deserialize<T[]>(jsonSerializerOptions);

        return result;
    }

    private static (bool Found, JsonElement? JsonElement) SelectJsonElement(this IJsonDocumentProvider jsonDocumentProvider,
      string jsonPath, JsonValueKind? jsonValueKind)
    {
        (bool found, IList<JsonElement>? jsonElements) = jsonDocumentProvider.SelectJsonElements(jsonPath, jsonValueKind);

        if (!found || jsonElements == null)
        {
          return (false, null);
        }

        if (jsonElements is not { Count: 1 })
        {
          return (false, null);
        }

        JsonElement jsonElement = jsonElements[0];

        return (true, jsonElement);
    }

    private static (bool Found, IList<JsonElement>? JsonElements) SelectJsonElements(this IJsonDocumentProvider jsonDocumentProvider,
        string jsonPath, JsonValueKind? jsonValueKind)
    {
        if (jsonDocumentProvider.JsonDocument == null)
        {
            return (false, null);
        }

        IList<JsonElement> localJsonElements =
            JsonSelector.Select(jsonDocumentProvider.JsonDocument.RootElement, jsonPath);

        if (localJsonElements.Count == 0)
        {
            return (false, null);
        }

        if (jsonValueKind.HasValue && localJsonElements.Any(e => e.ValueKind != jsonValueKind.Value))
        {
            return (false, null);
        }

        IList<JsonElement> jsonElements = localJsonElements;

        return (true, jsonElements);
    }
}
