namespace ConfirmSteps.Steps.Http.RequestBuilding;

using System.Collections;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Web;

using ConfirmSteps.Templating;

/// <summary>
/// Provides a fluent builder for constructing HTTP requests.
/// </summary>
public sealed class RequestBuilder : IHttpRequestMessageConverter
{
    private const string MediaTypeJson = "application/json";

    private RequestBuilder(HttpMethod method, TemplateString? baseUrl = null)
    {
        Method = method;
        BaseUrl = baseUrl;
    }

    /// <summary>
    /// Creates a builder for a DELETE request.
    /// </summary>
    /// <param name="baseUrl">The base URL for the request.</param>
    /// <returns>A new <see cref="RequestBuilder"/> instance.</returns>
    public static RequestBuilder Delete(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Delete, baseUrl);
    }

    /// <summary>
    /// Creates a builder for a GET request.
    /// </summary>
    /// <param name="baseUrl">The base URL for the request.</param>
    /// <returns>A new <see cref="RequestBuilder"/> instance.</returns>
    public static RequestBuilder Get(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Get, baseUrl);
    }

    /// <summary>
    /// Creates a builder for a HEAD request.
    /// </summary>
    /// <param name="baseUrl">The base URL for the request.</param>
    /// <returns>A new <see cref="RequestBuilder"/> instance.</returns>
    public static RequestBuilder Head(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Head, baseUrl);
    }

    /// <summary>
    /// Creates a builder for an OPTIONS request.
    /// </summary>
    /// <param name="baseUrl">The base URL for the request.</param>
    /// <returns>A new <see cref="RequestBuilder"/> instance.</returns>
    public static RequestBuilder Options(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Options, baseUrl);
    }

    /// <summary>
    /// Creates a builder for a PATCH request.
    /// </summary>
    /// <param name="baseUrl">The base URL for the request.</param>
    /// <returns>A new <see cref="RequestBuilder"/> instance.</returns>
    public static RequestBuilder Patch(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Patch, baseUrl);
    }

    /// <summary>
    /// Creates a builder for a POST request.
    /// </summary>
    /// <param name="baseUrl">The base URL for the request.</param>
    /// <returns>A new <see cref="RequestBuilder"/> instance.</returns>
    public static RequestBuilder Post(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Post, baseUrl);
    }

    /// <summary>
    /// Creates a builder for a PUT request.
    /// </summary>
    /// <param name="baseUrl">The base URL for the request.</param>
    /// <returns>A new <see cref="RequestBuilder"/> instance.</returns>
    public static RequestBuilder Put(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Put, baseUrl);
    }

    /// <summary>
    /// Creates a builder for a TRACE request.
    /// </summary>
    /// <param name="baseUrl">The base URL for the request.</param>
    /// <returns>A new <see cref="RequestBuilder"/> instance.</returns>
    public static RequestBuilder Trace(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Trace, baseUrl);
    }

    private TemplateString? BaseUrl { get; }

    private TemplateString? Body { get; set; }

    private JsonNode? JsonBody { get; set; }

    private TemplateString? Fragment { get; set; }

    private Dictionary<TemplateString, TemplateString> Headers { get; } = new();

    private HttpMethod Method { get; }

    private List<TemplateString> PathSegments { get; } = new();

    private List<KeyValuePair<TemplateString, TemplateString>> QueryString { get; } = new();

    /// <summary>
    /// Appends a path segment to the request URI.
    /// </summary>
    /// <param name="pathSegment">The path segment to append.</param>
    /// <returns>The current <see cref="RequestBuilder"/> for fluent chaining.</returns>
    public RequestBuilder AppendPathSegment(TemplateString pathSegment)
    {
        PathSegments.Add(pathSegment);

        return this;
    }

    /// <summary>
    /// Appends multiple path segments to the request URI.
    /// </summary>
    /// <param name="pathSegments">The path segments to append.</param>
    /// <returns>The current <see cref="RequestBuilder"/> for fluent chaining.</returns>
    public RequestBuilder AppendPathSegments(params TemplateString[] pathSegments)
    {
        if (pathSegments.Length > 0)
        {
            PathSegments.AddRange(pathSegments);
        }

        return this;
    }

    /// <summary>
    /// Sets the body of the request.
    /// </summary>
    /// <param name="body">The template string for the body.</param>
    /// <returns>The current <see cref="RequestBuilder"/> for fluent chaining.</returns>
    public RequestBuilder WithBody(TemplateString body)
    {
        if (JsonBody != null)
        {
            throw new InvalidOperationException(
                "The request already has a JSON body. A request has one body: use either "
                + $"{nameof(WithBody)} or {nameof(WithJsonBody)}.");
        }

        Body = body;
        return this;
    }

    /// <summary>
    /// Sets the request body from a JSON structure whose string values may be placeholders.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The alternative to <see cref="WithBody(TemplateString)"/>, and the one to prefer whenever the
    /// body is JSON. The document is <b>built and serialised</b> rather than assembled as text, so
    /// escaping is guaranteed by the serialiser instead of by whoever wrote the template: a value
    /// carrying a quote or a newline cannot break the document, nor inject one.
    /// </para>
    /// <para>
    /// A string value that is exactly one placeholder takes the <b>type of its variable</b> — a number
    /// stays a number, a collection becomes an array — which is what a text template cannot express.
    /// The quotes around it are how a placeholder is written inside JSON, not a claim that the result
    /// is a string.
    /// </para>
    /// <para>
    /// The template is a plain <see cref="JsonNode"/>, so a body read out of a step description passes
    /// through untouched, with no intermediate shape to define or map.
    /// </para>
    /// </remarks>
    /// <param name="body">The body template.</param>
    /// <returns>The current <see cref="RequestBuilder"/> for fluent chaining.</returns>
    public RequestBuilder WithJsonBody(JsonNode body)
    {
        if (Body != null)
        {
            throw new InvalidOperationException(
                "The request already has a text body. A request has one body: use either "
                + $"{nameof(WithBody)} or {nameof(WithJsonBody)}.");
        }

        JsonBody = body;
        return this;
    }

    /// <summary>
    /// Sets the body of the request from a file.
    /// </summary>
    /// <param name="path">The path to the file containing the body.</param>
    /// <returns>The current <see cref="RequestBuilder"/> for fluent chaining.</returns>
    public RequestBuilder WithBodyFile(string path)
    {
        TemplateString body = TemplateString.LoadFromFile(path);

        return WithBody(body);
    }

    /// <summary>
    /// Configures headers for the request.
    /// </summary>
    /// <param name="headers">A delegate to configure the headers.</param>
    /// <returns>The current <see cref="RequestBuilder"/> for fluent chaining.</returns>
    public RequestBuilder WithHeaders(Action<HeaderBuilder> headers)
    {
        HeaderBuilder builder = new();

        headers.Invoke(builder);

        if (builder.Headers.Count > 0)
        {
            foreach (KeyValuePair<TemplateString, TemplateString> header in builder.Headers)
            {
                Headers[header.Key] = header.Value;
            }
        }

        return this;
    }

    /// <summary>
    /// Configures the query string for the request.
    /// </summary>
    /// <param name="queryStringParameters">A delegate to configure the query string.</param>
    /// <returns>The current <see cref="RequestBuilder"/> for fluent chaining.</returns>
    public RequestBuilder WithQueryString(Action<QueryStringBuilder> queryStringParameters)
    {
        QueryStringBuilder builder = new();

        queryStringParameters.Invoke(builder);

        if (builder.QueryString.Count > 0)
        {
            QueryString.AddRange(builder.QueryString);
        }

        return this;
    }

    private void AddHeaders(HttpRequestMessage httpRequestMessage, IReadOnlyDictionary<string, object> vars)
    {
        if (Headers.Count == 0)
        {
            return;
        }

        bool haveContent = httpRequestMessage.Content != null;

        foreach (KeyValuePair<TemplateString, TemplateString> header in Headers)
        {
            string name = header.Key.Render(vars);
            string value = header.Value.Render(vars);

            if (!HttpHeaderExtensions.IsHttpContentHeader(name))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(name, value);
            }
            else if (haveContent && !string.Equals(HeaderNames.ContentType, name, StringComparison.OrdinalIgnoreCase))
            {
                httpRequestMessage.Content!.Headers.TryAddWithoutValidation(name, value);
            }
            else
            {
                // If the header is a content header but there is no content, or if it's a content-type header, we can't add it to the request.
                // In this case, we can choose to ignore it or throw an exception. Here, we'll ignore it.
            }
        }
    }

    HttpRequestMessage IHttpRequestMessageConverter.ToHttpRequestMessageConverter(Uri? baseAddress,
        IReadOnlyDictionary<string, object> vars)
    {
        EnsureEveryVariableResolved(vars);

        HttpRequestMessage httpRequestMessage = new()
        {
            Method = Method,
            RequestUri = ToRequestUri(baseAddress, vars),
            Content = ToRequestBody(vars),
        };

        AddHeaders(httpRequestMessage, vars);

        return httpRequestMessage;
    }

    /// <summary>
    /// Refuses to build a request whose templates expect a variable that has no value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rendering leaves an unknown placeholder in place, which suits a report and not a request: the
    /// placeholder would go out url-encoded and come back as a 400 or a 404, reported as the system
    /// under test misbehaving rather than as the broken correlation chain it is.
    /// </para>
    /// <para>
    /// Checked in one pass over every template the request carries, so all the missing variables are
    /// named at once. A variable present but null counts as missing, which is what rendering already
    /// does with it.
    /// </para>
    /// </remarks>
    private void EnsureEveryVariableResolved(IReadOnlyDictionary<string, object> vars)
    {
        List<UnresolvedTemplateVariable> unresolved = new();

        void Check(TemplateString? template, string location)
        {
            if (template == null)
            {
                return;
            }

            foreach (string name in template.ParameterNames)
            {
                if (!vars.TryGetValue(name, out object? value) || value == null)
                {
                    unresolved.Add(new UnresolvedTemplateVariable(name, location));
                }
            }
        }

        Check(BaseUrl, "base url");

        for (int i = 0; i < PathSegments.Count; i++)
        {
            Check(PathSegments[i], $"path segment {i + 1}");
        }

        foreach (KeyValuePair<TemplateString, TemplateString> parameter in QueryString)
        {
            string name = parameter.Key.Render(vars);

            Check(parameter.Key, "a query parameter name");
            Check(parameter.Value, $"query '{name}'");
        }

        foreach (KeyValuePair<TemplateString, TemplateString> header in Headers)
        {
            string name = header.Key.Render(vars);

            Check(header.Key, "a header name");
            Check(header.Value, $"header '{name}'");
        }

        Check(Body, "body");
        Check(Fragment, "fragment");

        // Walked node by node, so a missing variable is reported with its place in the document.
        JsonBodyTemplate.CollectUnresolved(JsonBody, vars, "$", unresolved);

        if (unresolved.Count > 0)
        {
            throw new UnresolvedTemplateVariableException(unresolved);
        }
    }

    /// <summary>
    /// Serialises the rendered JSON body, defaulting its content type to <c>application/json</c>.
    /// </summary>
    /// <remarks>
    /// An explicit Content-Type header still wins: a body may be JSON and be declared as a more
    /// specific media type, a problem document or a vendor type among them.
    /// </remarks>
    private HttpContent? ToJsonRequestBody(IReadOnlyDictionary<string, object> vars)
    {
        JsonNode? rendered = JsonBodyTemplate.Render(JsonBody, vars);

        if (rendered == null)
        {
            return null;
        }

        string contentType = Headers.TryGetValue(HeaderNames.ContentType,
            out TemplateString? headerContentType) && headerContentType != null
            ? headerContentType.Render(vars)
            : MediaTypeJson;

        StringContent stringContent = new(rendered.ToJsonString(), Encoding.UTF8, contentType);

        return stringContent;
    }

    private HttpContent? ToRequestBody(IReadOnlyDictionary<string, object> vars)
    {
        if (JsonBody != null)
        {
            return ToJsonRequestBody(vars);
        }

        if (Body == null)
        {
            return null;
        }

        string body = Body.Render(vars);
        bool haveContentTypeHeader =
            Headers.TryGetValue(HeaderNames.ContentType, out TemplateString? headerContentType);

        if (!haveContentTypeHeader || headerContentType == null)
        {
            return new StringContent(body);
        }

        string contentType = headerContentType.Render(vars);
        StringContent stringContent = new StringContent(body);

        stringContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        return stringContent;
    }

    private string ToRequestFragment(IReadOnlyDictionary<string, object> vars)
    {
        if (Fragment == null)
        {
            return string.Empty;
        }

        string fragment = HttpUtility.UrlEncode(Fragment.Render(vars));

        return fragment;
    }

    /// <summary>
    /// Builds the query string, repeating a parameter whose variable carries several values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A parameter declared once can go out several times: <c>modelIds={{MODEL_IDS}}</c> becomes
    /// <c>modelIds=1&amp;modelIds=2&amp;modelIds=3</c> when the variable holds three values. The count
    /// is therefore a property of the <b>data</b>, not of the request — which is what allows a run to
    /// vary it, say to measure what asking for thirty identifiers costs against five, without
    /// touching the description of the step.
    /// </para>
    /// <para>
    /// An empty collection produces no parameter at all. This is the only case in which a declared
    /// parameter disappears; a single value, empty string included, always produces one.
    /// </para>
    /// </remarks>
    private string ToRequestQueryString(IReadOnlyDictionary<string, object> vars)
    {
        if (QueryString.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new();

        foreach (KeyValuePair<TemplateString, TemplateString> queryStringParameter in QueryString)
        {
            string key = HttpUtility.UrlEncode(queryStringParameter.Key.Render(vars), Encoding.UTF8);

            foreach (string value in RenderQueryValues(queryStringParameter.Value, vars))
            {
                if (sb.Length > 0)
                {
                    sb.Append('&');
                }

                sb.Append(key).Append('=').Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
            }
        }

        string queryString = sb.ToString();

        return queryString;
    }

    /// <summary>
    /// The values one declared query parameter contributes: one, several, or none.
    /// </summary>
    private static IEnumerable<string> RenderQueryValues(TemplateString template,
        IReadOnlyDictionary<string, object> vars)
    {
        if (!TryGetMultiValued(template, vars, out IEnumerable? values) || values is null)
        {
            return [template.Render(vars)];
        }

        // ToString on each element rather than the collection: a single value and an element of a
        // collection must render identically, or a run varying the count would also change the format.
        return values.Cast<object?>()
            .Select(v => v?.ToString() ?? string.Empty)
            .ToList();
    }

    /// <summary>
    /// Whether the template stands for a variable that carries several values.
    /// </summary>
    /// <remarks>
    /// Only a template that is one placeholder and nothing else can: text built around a placeholder
    /// has no meaningful reading of a list. A string is not treated as a collection of characters, and
    /// a collection used inside surrounding text is refused rather than rendered as its type name,
    /// which is what it used to produce.
    /// </remarks>
    private static bool TryGetMultiValued(TemplateString template,
        IReadOnlyDictionary<string, object> vars, out IEnumerable? values)
    {
        values = null;

        if (template.ParameterNames.Count != 1)
        {
            return false;
        }

        if (!vars.TryGetValue(template.ParameterNames[0], out object? value)
            || value is null or string
            || value is not IEnumerable enumerable)
        {
            return false;
        }

        if (!template.IsSinglePlaceholder)
        {
            throw new MultiValuedTemplateVariableException(template.ParameterNames[0]);
        }

        values = enumerable;

        return true;
    }

    private Uri ToRequestUri(Uri? baseAddress, IReadOnlyDictionary<string, object> vars)
    {
        UriBuilder uriBuilder = baseAddress != null ? new UriBuilder(baseAddress) : new UriBuilder();

        if (BaseUrl != null)
        {
            string overrideBaseAddress = BaseUrl.Render(vars);
            uriBuilder = new UriBuilder(overrideBaseAddress);
        }

        uriBuilder.Path += string.Join("/", PathSegments.Select(ps => ps.Render(vars)));
        uriBuilder.Query = ToRequestQueryString(vars);
        uriBuilder.Fragment = ToRequestFragment(vars);

        Uri requestUri = uriBuilder.Uri;

        return requestUri;
    }
}
