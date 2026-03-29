namespace ConfirmSteps;

using ConfirmSteps.Steps.Http;
using ConfirmSteps.Steps.Http.RequestBuilding;

/// <summary>
/// Provides extension methods for adding HTTP-based steps to a scenario.
/// </summary>
public static class HttpStepExtensions
{
    /// <summary>
    /// Adds an HTTP-based step to the scenario.
    /// </summary>
    /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
    /// <param name="stepBuilderAppender">The step builder appender.</param>
    /// <param name="title">The title of the step.</param>
    /// <param name="requestBuilder">A function that returns a <see cref="RequestBuilder"/> configured for the request.</param>
    /// <param name="stepBuilder">An optional action to further configure the HTTP-based step.</param>
    /// <returns>The <see cref="IStepBuilderAppender{T}"/> for fluent chaining.</returns>
    public static IStepBuilderAppender<T> HttpStep<T>(this IStepBuilderAppender<T> stepBuilderAppender, string title,
        Func<RequestBuilder> requestBuilder,
        Action<HttpStepBuilder<T>>? stepBuilder = null)
        where T : class
    {
        HttpStepBuilder<T> httpStepBuilder = new(requestBuilder);

        stepBuilder?.Invoke(httpStepBuilder);

        stepBuilderAppender.Append(httpStepBuilder);

        return stepBuilderAppender;
    }
}
