namespace ConfirmSteps.Steps.Http.Rest;

using System.Net;
using System.Text.Json;
using ConfirmSteps.Steps.Http.Problems;

/// <summary>
/// Represents the result of a REST API call, containing the JSON response and potentially a JSON problem detail.
/// </summary>
public class RestApiResult : IJsonDocumentProvider, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestApiResult"/> class.
    /// </summary>
    /// <param name="innerResponse">The inner JSON response.</param>
    /// <param name="jsonResult">The JSON result document.</param>
    /// <param name="jsonProblem">The JSON problem document.</param>
    public RestApiResult(HttpResponseJson? innerResponse,
        JsonDocument? jsonResult = null, JsonDocument? jsonProblem = null)
    {
        InnerResponse = innerResponse;
        JsonResult = jsonResult;
        JsonProblem = jsonProblem;
    }

    /// <summary>
    /// Gets a value indicating whether the response contains a problem detail.
    /// </summary>
    public bool HaveProblem => JsonProblem != null;

    /// <summary>
    /// Gets a value indicating whether the response contains a result document (and no problem).
    /// </summary>
    public bool HaveResult => JsonProblem == null;

    /// <summary>
    /// Gets the JSON problem document, if any.
    /// </summary>
    public JsonDocument? JsonProblem { get; }

    /// <summary>
    /// Gets the JSON result document, if any.
    /// </summary>
    public JsonDocument? JsonResult { get; }

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
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

/// <summary>
/// Represents a typed result of a REST API call.
/// </summary>
/// <typeparam name="T">The type of the result data.</typeparam>
public sealed class RestApiResult<T> : RestApiResult
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestApiResult{T}"/> class.
    /// </summary>
    /// <param name="innerResponse">The inner JSON response.</param>
    /// <param name="jsonResult">The JSON result document.</param>
    /// <param name="jsonProblem">The JSON problem document.</param>
    /// <param name="result">The deserialized result object.</param>
    /// <param name="problem">The deserialized problem details object.</param>
    public RestApiResult(HttpResponseJson? innerResponse,
        JsonDocument? jsonResult = null, JsonDocument? jsonProblem = null,
        T? result = null, ProblemDetails? problem = null)
        : base(innerResponse, jsonResult, jsonProblem)
    {
        Result = result;
        Problem = problem;
    }

    /// <summary>
    /// Gets the deserialized problem details, if any.
    /// </summary>
    public ProblemDetails? Problem { get; }

    /// <summary>
    /// Gets the deserialized result object, if any.
    /// </summary>
    public T? Result { get; }
}
