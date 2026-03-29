namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Text.Json;

/// <summary>
/// Represents the result of parsing an HTTP response.
/// </summary>
/// <typeparam name="T">The type of the parsed response object.</typeparam>
public sealed class HttpResponseParseResult<T>
{
    /// <summary>
    /// Gets a failed parse result.
    /// </summary>
    public static HttpResponseParseResult<T> Failed { get; } = new() { Success = false };

    /// <summary>
    /// Gets the exception that occurred during parsing, if any.
    /// </summary>
    public JsonException? Exception { get; init; }

    /// <summary>
    /// Gets the parsed response object.
    /// </summary>
    public T? Response { get; init; }

    /// <summary>
    /// Gets a value indicating whether the parsing was successful.
    /// </summary>
    public bool Success { get; init; }
}
