namespace ConfirmSteps.Steps.Http.ResponseParsing;

/// <summary>
/// Defines an extractor that pulls data from an HTTP response into the scenario context.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public interface IHttpResponseExtractor<T>
    where T : class
{
    /// <summary>
    /// Extracts data from the HTTP response.
    /// </summary>
    /// <param name="stepContext">The step context.</param>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Extract(StepContext<T> stepContext, HttpResponseMessage response, CancellationToken cancellationToken);
}
