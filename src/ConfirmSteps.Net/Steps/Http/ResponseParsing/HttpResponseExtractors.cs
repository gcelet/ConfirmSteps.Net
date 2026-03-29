namespace ConfirmSteps.Steps.Http.ResponseParsing;

using ConfirmSteps.Steps.Http.Rest;

/// <summary>
/// Provides common extractor functions for HTTP responses.
/// </summary>
public static class HttpResponseExtractors
{
    /// <summary>
    /// Creates an extractor that pulls a boolean value from an HTTP header.
    /// </summary>
    /// <param name="headerName">The name of the header.</param>
    /// <returns>An extractor function.</returns>
    public static Func<HttpResponseMessage, object?> FromHeaderToBoolean(string headerName)
    {
        return response => response.SelectBoolean(headerName);
    }

    /// <summary>
    /// Creates an extractor that pulls a numeric value from an HTTP header.
    /// </summary>
    /// <param name="headerName">The name of the header.</param>
    /// <returns>An extractor function.</returns>
    public static Func<HttpResponseMessage, object?> FromHeaderToNumber(string headerName)
    {
        return response => response.SelectNumber(headerName);
    }

    /// <summary>
    /// Creates an extractor that pulls a string value from an HTTP header.
    /// </summary>
    /// <param name="headerName">The name of the header.</param>
    /// <returns>An extractor function.</returns>
    public static Func<HttpResponseMessage, object?> FromHeaderToString(string headerName)
    {
        return response => response.SelectString(headerName);
    }

    /// <summary>
    /// Creates an extractor that pulls a boolean value from the JSON body using a JSON path.
    /// </summary>
    /// <param name="jsonPath">The JSON path.</param>
    /// <returns>An extractor function.</returns>
    public static Func<HttpResponseJson, object?> FromJsonBodyToBoolean(string jsonPath)
    {
        return response => response.SelectBoolean(jsonPath);
    }

    /// <summary>
    /// Creates an extractor that pulls a numeric value from the JSON body using a JSON path.
    /// </summary>
    /// <param name="jsonPath">The JSON path.</param>
    /// <returns>An extractor function.</returns>
    public static Func<HttpResponseJson, object?> FromJsonBodyToNumber(string jsonPath)
    {
        return response => response.SelectNumber(jsonPath);
    }

    /// <summary>
    /// Creates an extractor that pulls a string value from the JSON body using a JSON path.
    /// </summary>
    /// <param name="jsonPath">The JSON path.</param>
    /// <returns>An extractor function.</returns>
    public static Func<HttpResponseJson, object?> FromJsonBodyToString(string jsonPath)
    {
        return response => response.SelectString(jsonPath);
    }
}
