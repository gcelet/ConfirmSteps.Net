namespace ConfirmSteps.Steps.Http.Rest;

using System.Net;
using System.Text.Json;
using ConfirmSteps.Steps.Http.Problems;

public class RestApiResult : IJsonDocumentProvider, IDisposable
{
    public RestApiResult(HttpResponseJson? innerResponse,
        JsonDocument? jsonResult = null, JsonDocument? jsonProblem = null)
    {
        InnerResponse = innerResponse;
        JsonResult = jsonResult;
        JsonProblem = jsonProblem;
    }

    public bool HaveProblem => JsonProblem != null;

    public bool HaveResult => JsonProblem == null;

    public JsonDocument? JsonProblem { get; }

    public JsonDocument? JsonResult { get; }

    public HttpStatusCode StatusCode => InnerResponse?.StatusCode ?? 0;

    private HttpResponseJson? InnerResponse { get; }

    /// <inheritdoc />
    JsonDocument? IJsonDocumentProvider.JsonDocument => JsonResult;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            InnerResponse?.Dispose();
        }
    }
}

public sealed class RestApiResult<T> : RestApiResult
    where T : class
{
    public RestApiResult(HttpResponseJson? innerResponse,
        JsonDocument? jsonResult = null, JsonDocument? jsonProblem = null,
        T? result = null, ProblemDetails? problem = null)
        : base(innerResponse, jsonResult, jsonProblem)
    {
        Result = result;
        Problem = problem;
    }

    public ProblemDetails? Problem { get; }

    public T? Result { get; }
}