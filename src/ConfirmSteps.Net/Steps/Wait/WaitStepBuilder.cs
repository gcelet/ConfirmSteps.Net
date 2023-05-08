namespace ConfirmSteps.Steps.Wait;

using ConfirmSteps.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class WaitStepBuilder<T> : IStepBuilder<T>
    where T : class
{
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
        services.TryAddSingleton<Random>(sp => new Random());
        return services;
    }
}