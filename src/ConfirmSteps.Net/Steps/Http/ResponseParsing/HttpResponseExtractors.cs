namespace ConfirmSteps.Steps.Http.ResponseParsing;

using ConfirmSteps.Steps.Http.Rest;

public static class HttpResponseExtractors
{
    public static Func<HttpResponseMessage, object?> FromHeaderToBoolean(string headerName)
    {
        return response => response.SelectBoolean(headerName);
    }

    public static Func<HttpResponseMessage, object?> FromHeaderToNumber(string headerName)
    {
        return response => response.SelectNumber(headerName);
    }

    public static Func<HttpResponseMessage, object?> FromHeaderToString(string headerName)
    {
        return response => response.SelectString(headerName);
    }

    public static Func<HttpResponseJson, object?> FromJsonBodyToBoolean(string jsonPath)
    {
        return response => response.SelectBoolean(jsonPath);
    }

    public static Func<HttpResponseJson, object?> FromJsonBodyToNumber(string jsonPath)
    {
        return response => response.SelectNumber(jsonPath);
    }

    public static Func<HttpResponseJson, object?> FromJsonBodyToString(string jsonPath)
    {
        return response => response.SelectString(jsonPath);
    }
}