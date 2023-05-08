namespace ConfirmSteps.Steps.Http.RequestBuilding;

using System.Net.Http.Headers;
using System.Text;
using System.Web;
using ConfirmSteps.Templating;

public sealed class RequestBuilder : IHttpRequestMessageConverter
{
    private RequestBuilder(HttpMethod method, TemplateString? baseUrl = null)
    {
        Method = method;
        BaseUrl = baseUrl;
    }

    public static RequestBuilder Delete(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Delete, baseUrl);
    }

    public static RequestBuilder Get(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Get, baseUrl);
    }

    public static RequestBuilder Head(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Head, baseUrl);
    }

    public static RequestBuilder Options(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Options, baseUrl);
    }

    public static RequestBuilder Patch(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Patch, baseUrl);
    }

    public static RequestBuilder Post(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Post, baseUrl);
    }

    public static RequestBuilder Put(TemplateString? baseUrl = null)
    {
        return new RequestBuilder(HttpMethod.Put, baseUrl);
    }

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

    public RequestBuilder AppendPathSegment(TemplateString pathSegment)
    {
        PathSegments.Add(pathSegment);

        return this;
    }

    public RequestBuilder AppendPathSegments(params TemplateString[] pathSegments)
    {
        if (pathSegments.Length > 0)
        {
            PathSegments.AddRange(pathSegments);
        }

        return this;
    }

    public RequestBuilder WithBody(TemplateString body)
    {
        Body = body;
        return this;
    }

    public RequestBuilder WithBodyFile(string path)
    {
        TemplateString body = TemplateString.LoadFromFile(path);

        return WithBody(body);
    }

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