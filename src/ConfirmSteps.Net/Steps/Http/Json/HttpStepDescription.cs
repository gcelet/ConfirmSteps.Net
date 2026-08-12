namespace ConfirmSteps.Steps.Http.Json;

using System.Text;
using System.Text.Json.Nodes;

using ConfirmSteps.Steps.Http.RequestBuilding;
using ConfirmSteps.Steps.Http.ResponseParsing;
using ConfirmSteps.Steps.Http.Rest;

/// <summary>
/// An HTTP step described as JSON rather than written in code.
/// </summary>
/// <remarks>
/// <para>
/// There is one HTTP step. This is a second way to <b>describe</b> it, not a second kind of step: a
/// description produces the same <see cref="HttpStepBuilder{T}"/> that code does, so everything that
/// works for one works for the other.
/// </para>
/// <para>
/// The shape the library reads:
/// </para>
/// <code>
/// {
///   "id": "courtesyvehicles.list",
///   "title": "Courtesy vehicles",
///   "request": {
///     "method": "GET",
///     "path": "api/courtesyvehicles",
///     "query":   [ { "name": "pageRange", "value": "1-20" } ],
///     "headers": { "Accept-language": "{{CULTURE_CODE}}" },
///     "body":    { "shopId": "{{SHOP_ID}}" }
///   },
///   "extract": [
///     { "var": "MODEL_IDS", "path": "$.items[*].modelId", "as": "stringList", "required": true }
///   ],
///   "verify": [ { "kind": "status", "expect": [200, 206] } ]
/// }
/// </code>
/// <para>
/// Everything else in the document is left alone. A host is expected to carry its own concerns there —
/// a category, the data a step needs, whatever its catalogue is made of — and reads them off the same
/// node it passed in. Entries under <c>verify</c> are equally untouched beyond their <c>kind</c>: what
/// a response ought to contain is the host's judgement, resolved through
/// <see cref="HttpStepVerifierRegistry{T}"/>.
/// </para>
/// </remarks>
public sealed class HttpStepDescription
{
    private HttpStepDescription(JsonNode document, string id, string title)
    {
        Document = document;
        Id = id;
        Title = title;
    }

    /// <summary>Gets the description as it was supplied, for a host to read its own properties from.</summary>
    public JsonNode Document { get; }

    /// <summary>Gets the identifier the description carries, or an empty string.</summary>
    public string Id { get; }

    /// <summary>Gets the title of the step: its own, or its identifier when it has no title.</summary>
    public string Title { get; }

    /// <summary>Reads a description from a JSON document already in memory.</summary>
    /// <param name="document">The description.</param>
    public static HttpStepDescription FromJson(JsonNode document)
    {
        if (document is not JsonObject root)
        {
            throw HttpStepDescriptionException.Invalid("$", "the description must be an object");
        }

        string id = ReadString(root, "id") ?? string.Empty;
        string title = ReadString(root, "title") ?? id;

        if (title.Length == 0)
        {
            throw HttpStepDescriptionException.Invalid("$", "a description needs a title or an id");
        }

        if (root["request"] is not JsonObject)
        {
            throw HttpStepDescriptionException.Invalid("$.request", "a request object is required");
        }

        return new HttpStepDescription(root, id, title);
    }

    /// <summary>Reads a description from a file, as a convenience over <see cref="FromJson"/>.</summary>
    /// <remarks>
    /// The library does not otherwise touch the disk: a host loading descriptions from anywhere else —
    /// an embedded resource, a database, a catalogue it assembled — uses <see cref="FromJson"/>.
    /// </remarks>
    /// <param name="path">Path of the description document.</param>
    public static HttpStepDescription LoadFromFile(string path)
    {
        string content = File.ReadAllText(path, Encoding.UTF8);
        JsonNode? document = JsonNode.Parse(content);

        return document == null
            ? throw HttpStepDescriptionException.Invalid(path, "the document is empty")
            : FromJson(document);
    }

    /// <summary>Builds the request the description declares, against its own root if it names one.</summary>
    public RequestBuilder BuildRequest() => BuildRequest(null);

    /// <summary>
    /// Builds the request the description declares, against a root the host supplies.
    /// </summary>
    /// <remarks>
    /// A description describes a <b>path</b>, not a host: the same one is meant to be played against a
    /// local server, a staging one and production, so the root belongs to whoever knows which
    /// environment is being exercised. A description may still name its own <c>baseUrl</c> — for an
    /// endpoint that genuinely lives elsewhere — and then it wins over what is passed here. With
    /// neither, the request is relative and the <c>HttpClient</c>'s own base address applies.
    /// </remarks>
    /// <param name="baseUrl">Root to build against, itself possibly a template.</param>
    public RequestBuilder BuildRequest(Templating.TemplateString? baseUrl)
    {
        JsonObject request = (JsonObject)Document["request"]!;
        RequestBuilder builder = ForMethod(ReadString(request, "method") ?? "GET",
            ReadString(request, "baseUrl"), baseUrl);

        if (ReadString(request, "path") is { Length: > 0 } path)
        {
            foreach (string segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                builder.AppendPathSegment(segment);
            }
        }

        if (request["query"] is JsonArray query)
        {
            builder.WithQueryString(q =>
            {
                foreach (JsonNode? entry in query)
                {
                    if (entry is not JsonObject parameter)
                    {
                        throw HttpStepDescriptionException.Invalid(
                            "$.request.query", "every entry must be an object with name and value");
                    }

                    q.Append(
                        ReadString(parameter, "name")
                        ?? throw HttpStepDescriptionException.Invalid(
                            "$.request.query[].name", "a name is required"),
                        ReadString(parameter, "value") ?? string.Empty);
                }
            });
        }

        if (request["headers"] is JsonObject headers)
        {
            builder.WithHeaders(h =>
            {
                foreach (KeyValuePair<string, JsonNode?> header in headers)
                {
                    h.Header(header.Key, header.Value?.ToString() ?? string.Empty);
                }
            });
        }

        if (request["body"] is { } body)
        {
            builder.WithJsonBody(body);
        }

        return builder;
    }

    /// <summary>
    /// Adds the extractions and the verifications the description declares to a step being built.
    /// </summary>
    /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
    /// <param name="builder">The step being built.</param>
    /// <param name="registry">The verifications the host makes available.</param>
    public void Apply<T>(HttpStepBuilder<T> builder, HttpStepVerifierRegistry<T> registry)
        where T : class
    {
        if (Document["extract"] is JsonArray extractions)
        {
            builder.Extract(extract =>
            {
                foreach (JsonNode? entry in extractions)
                {
                    AddExtraction(extract, entry);
                }
            });
        }

        if (Document["verify"] is not JsonArray verifications)
        {
            return;
        }

        foreach (JsonNode? entry in verifications)
        {
            if (entry is not JsonObject verification)
            {
                throw HttpStepDescriptionException.Invalid(
                    "$.verify", "every entry must be an object carrying a kind");
            }

            string kind = ReadString(verification, "kind")
                          ?? throw HttpStepDescriptionException.Invalid(
                              "$.verify[].kind", "a kind is required");

            registry.ApplyTo(kind, verification, builder);
        }
    }

    private static void AddExtraction<T>(HttpResponseExtractionBuilder<T> extract, JsonNode? entry)
        where T : class
    {
        if (entry is not JsonObject extraction)
        {
            throw HttpStepDescriptionException.Invalid(
                "$.extract", "every entry must be an object");
        }

        string variableName = ReadString(extraction, "var")
                              ?? throw HttpStepDescriptionException.Invalid(
                                  "$.extract[].var", "a variable name is required");

        string path = ReadString(extraction, "path")
                      ?? throw HttpStepDescriptionException.Invalid(
                          "$.extract[].path", "a path is required");

        ExtractedValueKind kind = ReadKind(ReadString(extraction, "as"));
        bool required = extraction["required"]?.GetValue<bool>() ?? false;

        extract.ToVars(variableName, response =>
        {
            object? value = Select(response, path, kind);

            // Required means "the chain must hold": say so here rather than let the step that
            // consumes the variable send an unresolved placeholder and blame the server.
            return required && IsMissing(value)
                ? throw new RequiredExtractionFailedException(variableName, path)
                : value;
        });
    }

    private static bool IsMissing(object? value)
        => value is null || (value is System.Collections.ICollection { Count: 0 });

    private static object? Select(HttpResponseJson response, string path, ExtractedValueKind kind)
    {
        IJsonDocumentProvider document = response;

        return kind switch
        {
            ExtractedValueKind.String => document.SelectString(path),
            ExtractedValueKind.StringList => document.SelectStringArray(path),
            ExtractedValueKind.Number => document.SelectNumber(path),
            ExtractedValueKind.NumberList => document.SelectNumberArray(path),
            ExtractedValueKind.Boolean => document.SelectBoolean(path),
            _ => document.SelectBooleanArray(path),
        };
    }

    private static ExtractedValueKind ReadKind(string? kind)
        => kind is null or { Length: 0 }
            ? ExtractedValueKind.String
            : Enum.TryParse(kind, ignoreCase: true, out ExtractedValueKind parsed)
                ? parsed
                : throw HttpStepDescriptionException.UnknownExtractionKind(kind);

    private static RequestBuilder ForMethod(string method, string? declaredBaseUrl,
        Templating.TemplateString? suppliedBaseUrl)
    {
        // What the description names wins: it is the case of an endpoint that genuinely lives
        // elsewhere, and the host cannot know that. Otherwise the host's root, which knows the
        // environment; and with neither, the HttpClient's own base address.
        Templating.TemplateString? root = declaredBaseUrl is { Length: > 0 }
            ? new Templating.TemplateString(declaredBaseUrl)
            : suppliedBaseUrl;

        return method.ToUpperInvariant() switch
        {
            "GET" => RequestBuilder.Get(root),
            "POST" => RequestBuilder.Post(root),
            "PUT" => RequestBuilder.Put(root),
            "PATCH" => RequestBuilder.Patch(root),
            "DELETE" => RequestBuilder.Delete(root),
            "HEAD" => RequestBuilder.Head(root),
            "OPTIONS" => RequestBuilder.Options(root),
            "TRACE" => RequestBuilder.Trace(root),
            _ => throw HttpStepDescriptionException.Invalid(
                "$.request.method", $"'{method}' is not an HTTP method this library builds"),
        };
    }

    private static string? ReadString(JsonObject owner, string property)
        => owner[property] is JsonValue value && value.TryGetValue(out string? text) ? text : null;
}
