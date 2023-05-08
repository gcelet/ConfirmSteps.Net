namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Text.Json;

public sealed class HttpResponseParseResult<T>
{
    public static HttpResponseParseResult<T> Failed { get; } = new() { Success = false };

    public JsonException? Exception { get; init; }

    public T? Response { get; init; }

    public bool Success { get; init; }
}