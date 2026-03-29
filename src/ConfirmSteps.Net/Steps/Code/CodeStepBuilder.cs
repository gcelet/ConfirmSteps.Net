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
    private Func<StepContext<T>, CancellationToken, Task<ConfirmStatus>> ExecuteFunc { get; set; } =
        (_, _) => Task.FromResult(ConfirmStatus.Indecisive);

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
        return new CodeStep<T>(ExecuteFunc);
    }

    /// <inheritdoc />
    IServiceCollection IStepBuilder<T>.RegisterServices(IServiceCollection services)
    {
        return services;
    }
}
