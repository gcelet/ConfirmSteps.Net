namespace ConfirmSteps;

using ConfirmSteps.Steps.Code;

/// <summary>
/// Provides extension methods for adding code-based steps to a scenario.
/// </summary>
public static class CodeStepExtensions
{
    /// <summary>
    /// Adds a code-based step to the scenario.
    /// </summary>
    /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
    /// <param name="stepBuilderAppender">The step builder appender.</param>
    /// <param name="title">The title of the step.</param>
    /// <param name="stepBuilder">An action to configure the code-based step.</param>
    /// <returns>The <see cref="IStepBuilderAppender{T}"/> for fluent chaining.</returns>
    public static IStepBuilderAppender<T> CodeStep<T>(this IStepBuilderAppender<T> stepBuilderAppender, string title,
        Action<CodeStepBuilder<T>> stepBuilder)
        where T : class
    {
        CodeStepBuilder<T> codeStepBuilder = new();

        stepBuilder.Invoke(codeStepBuilder);

        stepBuilderAppender.Append(codeStepBuilder);

        return stepBuilderAppender;
    }
}
