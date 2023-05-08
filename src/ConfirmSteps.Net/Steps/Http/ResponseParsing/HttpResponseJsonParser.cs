namespace ConfirmSteps.Steps.Http.ResponseParsing;

using System.Text.Json;
using ConfirmSteps.Steps.Http.Rest;

public class HttpResponseJsonParser : IHttpResponseJsonParser
{
    public HttpResponseJsonParser(JsonSerializerOptions options)
    {
        Options = options;
    }

    private JsonSerializerOptions Options { get; }

    /// <inheritdoc />
    public async Task<HttpResponseParseResult<HttpResponseJson>> TryParse(HttpResponseMessage? httpResponseMessage,
        CancellationToken cancellationToken)
    {
        if (httpResponseMessage == null)
        {
            return HttpResponseParseResult<HttpResponseJson>.Failed;
        }

        try
        {
            Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
            JsonDocument? response =
                await JsonSerializer.DeserializeAsync<JsonDocument>(stream, Options, cancellationToken);
            HttpResponseJson httpResponseJson = new(httpResponseMessage, response);

            return new HttpResponseParseResult<HttpResponseJson>
            {
                Success = true,
                Response = httpResponseJson,
            };
        }
        catch (JsonException exception)
        {
            return new HttpResponseParseResult<HttpResponseJson>
            {
                Success = false,
                Exception = exception,
            };
        }
        catch
        {
            return HttpResponseParseResult<HttpResponseJson>.Failed;
        }
    }
}