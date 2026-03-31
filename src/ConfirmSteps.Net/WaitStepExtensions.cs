namespace ConfirmSteps;

using ConfirmSteps.Steps.Wait;

/// <summary>
/// Provides extension methods for adding wait-based steps to a scenario.
/// </summary>
public static class WaitStepExtensions
{
  private const string DefaultTitle = "Wait";

  /// <summary>
  /// Adds a wait step with the specified delay range.
  /// </summary>
  /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
  /// <param name="stepBuilderAppender">The step builder appender.</param>
  /// <param name="delay">The delay range.</param>
  /// <returns>The <see cref="IStepBuilderAppender{T}"/> for fluent chaining.</returns>
  public static IStepBuilderAppender<T> WaitStep<T>(this IStepBuilderAppender<T> stepBuilderAppender,
    DelayRange delay)
    where T : class
  {
    return stepBuilderAppender.WaitStep(DefaultTitle, delay);
  }

  /// <summary>
  /// Adds a wait step with the specified delay range.
  /// </summary>
  /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
  /// <param name="stepBuilderAppender">The step builder appender.</param>
  /// <param name="title">The title of the step.</param>
  /// <param name="delay">The delay range.</param>
  /// <returns>The <see cref="IStepBuilderAppender{T}"/> for fluent chaining.</returns>
  public static IStepBuilderAppender<T> WaitStep<T>(this IStepBuilderAppender<T> stepBuilderAppender,
    string title, DelayRange delay)
    where T : class
  {
    stepBuilderAppender.Append(new WaitStepBuilder<T>(title, delay));

    return stepBuilderAppender;
  }

  /// <summary>
  /// Adds a wait step with the specified minimum and maximum delay in milliseconds.
  /// </summary>
  /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
  /// <param name="stepBuilderAppender">The step builder appender.</param>
  /// <param name="min">The minimum delay in milliseconds.</param>
  /// <param name="max">The maximum delay in milliseconds.</param>
  /// <returns>The <see cref="IStepBuilderAppender{T}"/> for fluent chaining.</returns>
  public static IStepBuilderAppender<T> WaitStep<T>(this IStepBuilderAppender<T> stepBuilderAppender,
    long min, long max)
    where T : class
  {
    DelayRange delayRange = new(min, max);

    return stepBuilderAppender.WaitStep(DefaultTitle, delayRange);
  }

  /// <summary>
  /// Adds a wait step with the specified minimum and maximum delay in milliseconds.
  /// </summary>
  /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
  /// <param name="stepBuilderAppender">The step builder appender.</param>
  /// <param name="title">The title of the step.</param>
  /// <param name="min">The minimum delay in milliseconds.</param>
  /// <param name="max">The maximum delay in milliseconds.</param>
  /// <returns>The <see cref="IStepBuilderAppender{T}"/> for fluent chaining.</returns>
  public static IStepBuilderAppender<T> WaitStep<T>(this IStepBuilderAppender<T> stepBuilderAppender,
    string title, long min, long max)
    where T : class
  {
    DelayRange delayRange = new(min, max);

    return stepBuilderAppender.WaitStep(title, delayRange);
  }
}
