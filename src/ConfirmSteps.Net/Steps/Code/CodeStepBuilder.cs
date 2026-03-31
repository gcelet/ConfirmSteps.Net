namespace ConfirmSteps.Steps.Code;

using ConfirmSteps.Internal;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides a builder for creating a code-based step.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public sealed class CodeStepBuilder<T> : IStepBuilder<T>
  where T : class
{
  /// <summary>
  /// Initializes a new instance of the <see cref="CodeStepBuilder{T}"/> class.
  /// </summary>
  /// <param name="title">The title of the step.</param>
  public CodeStepBuilder(string title)
  {
    Title = title;
  }

  private Func<StepContext<T>, CancellationToken, Task<ConfirmStatus>> ExecuteFunc { get; set; } =
    (_, _) => Task.FromResult(ConfirmStatus.Indecisive);

  private string Title { get; }

  /// <summary>
  /// Configures the asynchronous execution logic for the step.
  /// </summary>
  /// <param name="execute">The function to execute.</param>
  /// <returns>The current <see cref="CodeStepBuilder{T}"/> for fluent chaining.</returns>
  public CodeStepBuilder<T> Execute(Func<StepContext<T>, CancellationToken, Task<ConfirmStatus>> execute)
  {
    ExecuteFunc = execute;

    return this;
  }

  /// <summary>
  /// Configures the synchronous execution logic for the step.
  /// </summary>
  /// <param name="execute">The function to execute.</param>
  /// <returns>The current <see cref="CodeStepBuilder{T}"/> for fluent chaining.</returns>
  public CodeStepBuilder<T> Execute(Func<StepContext<T>, ConfirmStatus> execute)
  {
    ExecuteFunc = (c, _) =>
    {
      ConfirmStatus confirmStatus = execute(c);
      return Task.FromResult(confirmStatus);
    };

    return this;
  }

  /// <inheritdoc />
  IStep<T> IStepBuilder<T>.Build()
  {
    return new CodeStep<T>(Title, ExecuteFunc);
  }

  /// <inheritdoc />
  IServiceCollection IStepBuilder<T>.RegisterServices(IServiceCollection services)
  {
    return services;
  }
}
