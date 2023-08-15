namespace ConfirmSteps.Net.Tests.Http;

using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConfirmSteps.Steps.Http.Problems;
using ConfirmSteps.Steps.Http.Rest;

public static class Extensions
{
    public static HttpResponseJson ToHttpResponseJson(this JsonObject jsonObject,
        HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        JsonDocument? jsonDocument = jsonObject.Deserialize<JsonDocument>();
        HttpResponseMessage httpResponseMessage = jsonObject.ToHttpResponseMessage(httpStatusCode);
        HttpResponseJson httpResponseJson = new(httpResponseMessage, jsonDocument);

        return httpResponseJson;
    }

    public static RestApiResult ToRestApiResult(this JsonObject jsonObject,
        HttpStatusCode httpStatusCode = HttpStatusCode.OK,
        bool isProblem = false)
    {
        HttpResponseJson httpResponseJson = jsonObject.ToHttpResponseJson(httpStatusCode);
        JsonDocument? jsonDocument = jsonObject.Deserialize<JsonDocument>();
        RestApiResult restApiResult = new(httpResponseJson,
            isProblem ? null : jsonDocument,
            isProblem ? jsonDocument : null);

        return restApiResult;
    }

    public static RestApiResult<T> ToRestApiResult<T>(this JsonObject jsonObject,
        HttpStatusCode httpStatusCode = HttpStatusCode.OK,
        bool isProblem = false)
        where T : class
    {
        HttpResponseJson httpResponseJson = jsonObject.ToHttpResponseJson(httpStatusCode);
        JsonDocument? jsonDocument = jsonObject.Deserialize<JsonDocument>();
        T? result = isProblem
            ? null
            : jsonDocument?.Deserialize<T>();
        ProblemDetails? problemDetails = isProblem
            ? jsonDocument?.Deserialize<ProblemDetails>()
            : null;
        RestApiResult<T> restApiResult = new(httpResponseJson,
            isProblem ? null : jsonDocument,
            isProblem ? jsonDocument : null,
            result, problemDetails);

        return restApiResult;
    }

    private static HttpResponseMessage ToHttpResponseMessage(this JsonObject jsonObject,
        HttpStatusCode httpStatusCode)
    {
        string json = jsonObject.ToJsonString();
        HttpResponseMessage httpResponseMessage = new(httpStatusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        return httpResponseMessage;
    }
}