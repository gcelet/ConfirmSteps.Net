namespace ConfirmSteps.Steps.Http.RequestBuilding;

using System.Net.Http.Headers;
using System.Text;
using System.Web;
using ConfirmSteps.Templating;

/// <summary>
/// Provides a fluent builder for constructing HTTP requests.
/// </summary>
public sealed class RequestBuilder : IHttpRequestMessageConverter
{
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
        Body = body;
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
        }
    }

    HttpRequestMessage IHttpRequestMessageConverter.ToHttpRequestMessageConverter(Uri? baseAddress,
        IReadOnlyDictionary<string, object> vars)
    {
        HttpRequestMessage httpRequestMessage = new()
        {
            Method = Method,
            RequestUri = ToRequestUri(baseAddress, vars),
            Content = ToRequestBody(vars),
        };

        AddHeaders(httpRequestMessage, vars);

        return httpRequestMessage;
    }

    private HttpContent? ToRequestBody(IReadOnlyDictionary<string, object> vars)
    {
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

    private string ToRequestQueryString(IReadOnlyDictionary<string, object> vars)
    {
        if (QueryString.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new();

        for (int i = 0; i < QueryString.Count; i++)
        {
            if (i > 0)
            {
                sb.Append('&');
            }

            KeyValuePair<TemplateString, TemplateString> queryStringParameter = QueryString[i];
            string key = HttpUtility.UrlEncode(queryStringParameter.Key.Render(vars), Encoding.UTF8);
            string value = HttpUtility.UrlEncode(queryStringParameter.Value.Render(vars), Encoding.UTF8);

            sb.Append(key).Append('=').Append(value);
        }

        string queryString = sb.ToString();

        return queryString;
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
