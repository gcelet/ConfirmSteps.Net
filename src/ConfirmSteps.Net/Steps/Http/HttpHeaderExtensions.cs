namespace ConfirmSteps.Steps.Http;

using System.Globalization;

internal static class HttpHeaderExtensions
{
    private static readonly HashSet<string> HttpContentHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        HeaderNames.Allow,
        HeaderNames.ContentRange,
        HeaderNames.ContentDisposition,
        HeaderNames.ContentEncoding,
        HeaderNames.ContentLanguage,
        HeaderNames.ContentLength,
        HeaderNames.ContentLocation,
        HeaderNames.ContentMD5,
        HeaderNames.ContentType,
        HeaderNames.Expires,
        HeaderNames.LastModified
    };

    internal static bool IsHttpContentHeader(string header)
    {
        return HttpContentHeaders.Contains(header);
    }

    internal static bool? SelectBoolean(this HttpResponseMessage httpResponseMessage, string name)
    {
        string[]? values = httpResponseMessage.SelectStringValues(name);

        if (values == null || values.Length != 1)
        {
            return null;
        }

        if (!bool.TryParse(values[0], out bool b))
        {
            return null;
        }

        return b;
    }

    internal static decimal? SelectNumber(this HttpResponseMessage httpResponseMessage, string name)
    {
        string[]? values = httpResponseMessage.SelectStringValues(name);

        if (values == null || values.Length != 1)
        {
            return null;
        }

        if (!decimal.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal d))
        {
            return null;
        }

        return d;
    }

    internal static string? SelectString(this HttpResponseMessage httpResponseMessage, string name)
    {
        string[]? values = httpResponseMessage.SelectStringValues(name);

        if (values == null)
        {
            return null;
        }

        return values.Length == 1 ? values[0] : string.Join(';', values);
    }

    internal static string[]? SelectStringValues(this HttpResponseMessage httpResponseMessage, string name)
    {
        bool haveHeader = !IsHttpContentHeader(name)
            ? httpResponseMessage.Headers.TryGetValues(name, out IEnumerable<string>? values)
            : httpResponseMessage.Content.Headers.TryGetValues(name, out values);

        if (!haveHeader || values == null)
        {
            return null;
        }

        return values.ToArray();
    }
}