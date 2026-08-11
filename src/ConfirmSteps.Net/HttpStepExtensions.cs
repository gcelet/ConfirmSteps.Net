namespace ConfirmSteps;

using ConfirmSteps.Steps.Http;
using ConfirmSteps.Steps.Http.Json;
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
        HttpStepBuilder<T> httpStepBuilder = new(title, requestBuilder);

        stepBuilder?.Invoke(httpStepBuilder);

        stepBuilderAppender.Append(httpStepBuilder);

        return stepBuilderAppender;
    }

    /// <summary>
    /// Adds an HTTP-based step described as JSON rather than written in code.
    /// </summary>
    /// <remarks>
    /// The same step as the overload above, described differently: a description produces the same
    /// <see cref="HttpStepBuilder{T}"/>, so anything that works for a step written in code works here.
    /// The verifications a description names come from <paramref name="registry"/>, because what a
    /// response ought to contain is the host's judgement and not this library's.
    /// </remarks>
    /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
    /// <param name="stepBuilderAppender">The step builder appender.</param>
    /// <param name="description">The step description.</param>
    /// <param name="registry">The verifications the host makes available to descriptions.</param>
    /// <param name="stepBuilder">An optional action to further configure the step.</param>
    /// <returns>The <see cref="IStepBuilderAppender{T}"/> for fluent chaining.</returns>
    public static IStepBuilderAppender<T> HttpStep<T>(this IStepBuilderAppender<T> stepBuilderAppender,
        HttpStepDescription description,
        HttpStepVerifierRegistry<T> registry,
        Action<HttpStepBuilder<T>>? stepBuilder = null)
        where T : class
        => stepBuilderAppender.HttpStep(description, registry, description.Title, stepBuilder);

    /// <summary>
    /// Adds an HTTP-based step described as JSON, under a title of the host's choosing.
    /// </summary>
    /// <remarks>
    /// The overload a host numbering its steps uses: a description carries the title it was written
    /// with, and a catalogue often wants its own — an ordinal, a group, whatever its report groups by.
    /// </remarks>
    /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
    /// <param name="stepBuilderAppender">The step builder appender.</param>
    /// <param name="description">The step description.</param>
    /// <param name="registry">The verifications the host makes available to descriptions.</param>
    /// <param name="title">The title of the step, replacing the one the description carries.</param>
    /// <param name="stepBuilder">An optional action to further configure the step.</param>
    /// <returns>The <see cref="IStepBuilderAppender{T}"/> for fluent chaining.</returns>
    public static IStepBuilderAppender<T> HttpStep<T>(this IStepBuilderAppender<T> stepBuilderAppender,
        HttpStepDescription description,
        HttpStepVerifierRegistry<T> registry,
        string title,
        Action<HttpStepBuilder<T>>? stepBuilder = null)
        where T : class
    {
        HttpStepBuilder<T> httpStepBuilder = new(title, description.BuildRequest);

        description.Apply(httpStepBuilder, registry);

        stepBuilder?.Invoke(httpStepBuilder);

        stepBuilderAppender.Append(httpStepBuilder);

        return stepBuilderAppender;
    }
}
