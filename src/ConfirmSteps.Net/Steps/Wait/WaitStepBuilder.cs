namespace ConfirmSteps.Steps.Wait;

using ConfirmSteps.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Provides a builder for creating a wait step.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public class WaitStepBuilder<T> : IStepBuilder<T>
  where T : class
{
  /// <summary>
  /// Initializes a new instance of the <see cref="WaitStepBuilder{T}"/> class.
  /// </summary>
  /// <param name="title">The title of the step.</param>
  /// <param name="delay">The delay range.</param>
  public WaitStepBuilder(string title, DelayRange delay)
  {
    Title = title;
    Delay = delay;
  }

  private DelayRange Delay { get; }

  private string Title { get; }

  /// <inheritdoc />
  IStep<T> IStepBuilder<T>.Build()
  {
    return new WaitStep<T>(Title, Delay);
  }

  /// <inheritdoc />
  IServiceCollection IStepBuilder<T>.RegisterServices(IServiceCollection services)
  {
    // Registered as a singleton and resolved by every wait step, so it is shared across whatever
    // concurrency the host applies. System.Random instance methods are not thread-safe, which made
    // a single Scenario<T> unsafe to run from several threads at once — a wait step sits between
    // every pair of steps, so the race was continuous rather than occasional.
    // Random.Shared is thread-safe; TryAdd still lets a caller register their own instance.
    services.TryAddSingleton(Random.Shared);
    return services;
  }
}
