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
    /// <param name="delay">The delay range.</param>
    public WaitStepBuilder(DelayRange delay)
    {
        Delay = delay;
    }

    private DelayRange Delay { get; }

    /// <inheritdoc />
    IStep<T> IStepBuilder<T>.Build()
    {
        return new WaitStep<T>(Delay);
    }

    /// <inheritdoc />
    IServiceCollection IStepBuilder<T>.RegisterServices(IServiceCollection services)
    {
        services.TryAddSingleton(new Random());
        return services;
    }
}
